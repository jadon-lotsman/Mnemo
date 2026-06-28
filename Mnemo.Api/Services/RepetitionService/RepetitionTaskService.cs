using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mnemo.Contracts.Repetition.Results;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Services.RepetitionService.Factories;
using Mnemo.Services.RepetitionService.Strategies;
using Mnemo.Services.RepetitionService.Providers.DestructorProviders;
using Mnemo.Services.RepetitionService.Providers.TaskTypeProviders;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService
{
    public class RepetitionTaskService
    {
        private readonly AppDbContext _context;
        private readonly TaskQueries _taskQueries;

        private readonly AccountQueries _accountQueries;
        private readonly VocabularyQueries _vocabularyQueries;

        private readonly RepetitionStateService _stateService;

        private readonly ITaskTypeProvider _typeProvider;
        private readonly IDistractorProvider _distractorProvider;


        public RepetitionTaskService(AppDbContext context, AccountQueries accountQueries, TaskQueries taskQueries, VocabularyQueries vocabularyQueries, RepetitionStateService stateService, ITaskTypeProvider typeProvider, IDistractorProvider distractorProvider)
        {
            _context = context;

            _accountQueries = accountQueries;
            _taskQueries = taskQueries;
            _vocabularyQueries = vocabularyQueries;

            _stateService = stateService;

            _typeProvider = typeProvider;
            _distractorProvider = distractorProvider;
        }



        public async Task<RequestResult<bool>> ExistsRepetitionAsync(int userId)
        {
            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<bool>.Failure(ErrorCode.UserNotFound);

            bool inProcess = await _taskQueries.ExistsByUserId(userId);

            if (inProcess) return RequestResult<bool>.Success(true);
            else return RequestResult<bool>.Success(false);
        }



        public async Task<RequestResult<List<RepetitionTask>>> StartRepetitionAsync(int userId, string mode)
        {
            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<List<RepetitionTask>>.Failure(ErrorCode.UserNotFound);

            if (await _taskQueries.ExistsByUserId(userId))
                return RequestResult<List<RepetitionTask>>.Success(await _taskQueries.GetByUserIdQuery(userId).ToListAsync());

            IRepetitionTaskStrategy? strategy = mode switch
            {
                "fast" => new FastRepetitionTaskStrategy(new RepetitionTaskFactory(_distractorProvider), _typeProvider, _vocabularyQueries),
                "planned" => new PlannedRepetitionTaskStrategy(new RepetitionTaskFactory(_distractorProvider), _typeProvider, _vocabularyQueries),
                _ => null
            };

            if (strategy == null)
                return RequestResult<List<RepetitionTask>>.Failure(ErrorCode.InvalidData);


            var tasks = await strategy.GetTasksAsync(userId);

            if (!tasks.Any())
                return RequestResult<List<RepetitionTask>>.Failure(ErrorCode.TaskGenerationFailed);

            await _context.RepetitionTasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();

            return RequestResult<List<RepetitionTask>>.Success(tasks);
        }

        public async Task<RequestResult<RepetitionResultResponse>> FinishRepetitionAsync(int userId)
        {
            if (!await _taskQueries.ExistsByUserId(userId))
                return RequestResult<RepetitionResultResponse>.Failure(ErrorCode.RepetitionNotFound);


            var tasks = await _taskQueries.GetByUserIdQuery(userId).ToListAsync();
            

            var totalTime = TimeSpan.Zero;
            foreach (var task in tasks)
                totalTime += task.ElapsedTime;

            var averageTime = tasks.Count > 0? totalTime / tasks.Count : TimeSpan.Zero;


            var entryIdToQuality = new Dictionary<int, double>();
            var taskResults = new List<TaskResultResponse>();

            int totalTasks = tasks.Count;
            int correctTaskCounter = 0;

            foreach (var task in tasks)
            {
                double quality = task.GetQuality(averageTime);
                bool isCorrect = false;

                if (!entryIdToQuality.ContainsKey(task.AsessmentEntryId))
                    entryIdToQuality.Add(task.AsessmentEntryId, quality);

                if (SM2Helper.IsPassingQuality(quality))
                {
                    isCorrect = true;
                    correctTaskCounter++;
                }

                taskResults.Add(new TaskResultResponse()
                {
                    Id = task.Id,
                    Quality = quality,
                    IsCorrect = isCorrect,
                    CorrectAnswer = task.GetCorrect()
                });
            }

            _context.RepetitionTasks.RemoveRange(tasks);
            await _context.SaveChangesAsync();

            var record = await _stateService.RecordQualityRepetitionStateAsync(userId, entryIdToQuality);

            if (!record.IsSuccess)
                return RequestResult<RepetitionResultResponse>.Failure(record.ErrorCode!.Value, record.ErrorMessage);


            var response = new RepetitionResultResponse()
            {
                Correct = correctTaskCounter,
                Total = totalTasks,
                Percent = totalTasks > 0 ? (int) Math.Round((double) correctTaskCounter / totalTasks * 100) : 0,
                TotalTimeMilliseconds = (int) totalTime.TotalMilliseconds,
                TaskResults = taskResults.ToArray()
            };

            return RequestResult<RepetitionResultResponse>.Success(response);
        }

        public async Task<RequestResult<RepetitionTask>> SubmitRepetitionTaskAnswerAsync(int userId, int taskId, string answer, TimeSpan elapsedTime)
        {
            var task = await _taskQueries.GetTaskByIdAsync(userId, taskId);

            if (task == null)
                return RequestResult<RepetitionTask>.Failure(ErrorCode.TaskNotFound);


            task.SubmitAnswer(answer, elapsedTime);

            await _context.SaveChangesAsync();

            return RequestResult<RepetitionTask>.Success(task);
        }
    }
}
