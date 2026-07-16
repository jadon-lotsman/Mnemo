using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mnemo.Contracts.Repetition.Results;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Services.RepetitionService.Strategies;
using Mnemo.Shared;

namespace Mnemo.Services.RepetitionService
{
    public class RepetitionTaskService
    {
        private readonly IOptions<RepetitionOptions> _options;

        private readonly AppDbContext _context;
        private readonly TaskQueries _taskQueries;

        private readonly AccountQueries _accountQueries;

        private readonly RepetitionStateService _stateService;

        private readonly FastRepetitionTaskStrategy _fastStrategy;
        private readonly PlannedRepetitionTaskStrategy _plannedStrategy;


        public RepetitionTaskService(
            IOptions<RepetitionOptions> options,
            AppDbContext context,
            AccountQueries accountQueries,
            TaskQueries taskQueries,
            RepetitionStateService stateService,
            FastRepetitionTaskStrategy fastStrategy,
            PlannedRepetitionTaskStrategy plannedStrategy)
        {
            _options = options;

            _context = context;

            _accountQueries = accountQueries;
            _taskQueries = taskQueries;

            _stateService = stateService;

            _fastStrategy = fastStrategy;
            _plannedStrategy = plannedStrategy;
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
                "fast" => _fastStrategy,
                "planned" => _plannedStrategy,
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

            var averageTime = tasks.Count > 0 ? totalTime / tasks.Count : TimeSpan.Zero;


            var entryIdToQuality = new Dictionary<int, double>();
            var taskResults = new List<TaskResultResponse>();

            int totalTasks = tasks.Count;
            int correctTaskCounter = 0;

            foreach (var task in tasks)
            {
                double quality = task.GetQuality(averageTime);
                bool isCorrect = false;

                if (!entryIdToQuality.ContainsKey(task.VocabularyEntryId))
                    entryIdToQuality.Add(task.VocabularyEntryId, quality);

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

            await _stateService.BalanceRepetitionStateAsync(userId, _options.Value.RepetitionTaskCount);

            if (!record.IsSuccess)
                return RequestResult<RepetitionResultResponse>.Failure(record.ErrorCode!.Value, record.ErrorMessage);


            var response = new RepetitionResultResponse()
            {
                Correct = correctTaskCounter,
                Total = totalTasks,
                Percent = totalTasks > 0 ? (int)Math.Round((double)correctTaskCounter / totalTasks * 100) : 0,
                TotalTimeMilliseconds = (int)totalTime.TotalMilliseconds,
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
