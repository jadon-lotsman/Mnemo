using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;
using Mnemo.Services.Strategies;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services
{
    public class RepetitionTaskService
    {
        private AppDbContext _context;
        private TaskQueries _taskQueries;
        private AccountQueries _accountQueries;
        private VocabularyQueries _vocabularyQueries;

        private RepetitionStateService _stateService;



        public RepetitionTaskService(AppDbContext context, AccountQueries accountQueries, TaskQueries taskQueries, VocabularyQueries vocabularyQueries, RepetitionStateService stateService)
        {
            _context = context;

            _accountQueries = accountQueries;
            _taskQueries = taskQueries;
            _vocabularyQueries = vocabularyQueries;

            _stateService = stateService;
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
                "fast" => new RandomRepetitionTaskStrategy(_vocabularyQueries),
                "planned" => new PlannedRepetitionTaskStrategy(_vocabularyQueries),
                _ => null
            };

            if (strategy == null)
                return RequestResult<List<RepetitionTask>>.Failure(ErrorCode.InvalidData);


            await _stateService.CreateNonExistentRepetitionStatesAsync(userId);

            var tasks = await strategy.GetTasksAsync(userId);

            if (!tasks.Any())
                return RequestResult<List<RepetitionTask>>.Failure(ErrorCode.TaskGenerationFailed);

            await _context.RepetitionTasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();

            return RequestResult<List<RepetitionTask>>.Success(tasks);
        }

        public async Task<RequestResult<RepetitionResult>> FinishRepetitionAsync(int userId)
        {
            if (!await _taskQueries.ExistsByUserId(userId))
                return RequestResult<RepetitionResult>.Failure(ErrorCode.RepetitionNotFound);


            var tasks = await _taskQueries.GetByUserIdQuery(userId).ToListAsync();

            var averageTime = CalcAverageTime(tasks);
            var entryIdToQuality = new Dictionary<int, double>();

            int correctTaskCounter = 0;

            foreach (var task in tasks)
            {
                double quality = task.ComputeQuality(averageTime);


                entryIdToQuality.Add(task.AsessmentEntryId, quality);

                if (SM2Helper.IsPassingQuality(quality))
                    correctTaskCounter++;
            }

            var recordResult = await _stateService.RecordQualityRepetitionStateAsync(userId, entryIdToQuality);

            RepetitionResult result = new RepetitionResult(correctTaskCounter, []);

            _context.RepetitionResults.Add(result);
            _context.RepetitionTasks.RemoveRange(tasks);
            await _context.SaveChangesAsync();

            if (!recordResult.IsSuccess)
                return RequestResult<RepetitionResult>.Failure(recordResult.ErrorCode!.Value, recordResult.ErrorMessage);

            return RequestResult<RepetitionResult>.Success(result);
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

        public TimeSpan CalcAverageTime(ICollection<RepetitionTask> tasks)
        {
            var sum = TimeSpan.Zero;
            foreach (var task in tasks)
                sum += task.ElapsedTime;

            return sum / tasks.Count;
        }
    }
}
