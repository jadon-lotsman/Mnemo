using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;
using Mnemo.Services.Strategies;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services
{
    public class RepetitionSessionService
    {
        private AppDbContext _context;
        private AccountQueries _accountQueries;
        private SessionQueries _sessionQueries;
        private VocabularyQueries _vocabularyQueries;

        private RepetitionStateService _stateService;



        public RepetitionSessionService(AppDbContext context, AccountQueries accountQueries, SessionQueries sessionQueries, VocabularyQueries vocabularyQueries, RepetitionStateService stateService)
        {
            _context = context;

            _accountQueries = accountQueries;
            _sessionQueries = sessionQueries;
            _vocabularyQueries = vocabularyQueries;

            _stateService = stateService;
        }



        public async Task<RequestResult<RepetitionSession>> GetRepetitionSessionStatusAsync(int userId)
        {
            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<RepetitionSession>.Failure(ErrorCode.UserNotFound);

            var session = await _sessionQueries.GetByUserIdAsync(userId);

            if (session == null) return RequestResult<RepetitionSession>.Failure(ErrorCode.SessionNotFound);
            else return RequestResult<RepetitionSession>.Failure(ErrorCode.DuplicateSession);
        }



        public async Task<RequestResult<RepetitionSession>> StartRepetitionSessionAsync(int userId, string mode)
        {
            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<RepetitionSession>.Failure(ErrorCode.UserNotFound);

            if (await _sessionQueries.ExistsByUserId(userId))
                return RequestResult<RepetitionSession>.Failure(ErrorCode.DuplicateSession, "Session already exists");

            IRepetitionTaskStrategy? strategy = mode switch
            {
                "fast" => new RandomRepetitionTaskStrategy(_vocabularyQueries),
                "planned" => new PlannedRepetitionTaskStrategy(_vocabularyQueries),
                _ => null
            };

            if (strategy == null)
                return RequestResult<RepetitionSession>.Failure(ErrorCode.InvalidData);


            await _stateService.CreateNonExistentRepetitionStatesAsync(userId);

            var tasks = await strategy.GetTasksAsync(userId);

            if (!tasks.Any())
                return RequestResult<RepetitionSession>.Failure(ErrorCode.TaskGenerationFailed);

            var session = new RepetitionSession(userId, tasks);

            await _context.RepetitionSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return RequestResult<RepetitionSession>.Success(session);
        }

        public async Task<RequestResult<RepetitionResult>> FinishRepetitionSessionAsync(int userId)
        {
            var session = await _sessionQueries.GetByUserIdAsync(userId);

            if (session == null)
                return RequestResult<RepetitionResult>.Failure(ErrorCode.SessionNotFound);


            var tasks = await _sessionQueries.GetTasksByUserIdAsync(userId);

            session.FinishedAt = DateTime.UtcNow;


            var rawEntriesIds = tasks.Select(t => t.BaseVocabularyEntryId).ToList();
            var rawEntriesDict = await _vocabularyQueries.GetDictByIdsAsync(userId, rawEntriesIds);
            int missedEntriesCounter = 0;
            var existEntries = new List<VocabularyEntry>();

            int correctTaskCounter = 0;
            var entryIdToQuality = new Dictionary<int, double>();

            foreach (var task in tasks)
            {
                if (rawEntriesDict.TryGetValue(task.BaseVocabularyEntryId, out var entry) && entry != null)
                {
                    existEntries.Add(entry);

                    double similarity;

                    if (task.IsForwardQuestion)
                        similarity = entry.Translations.Max(task.UserAnswer.ComputeLevenshteinSimilarity);
                    else
                        similarity = task.UserAnswer.ComputeLevenshteinSimilarity(entry.Foreign);


                    double quality = SM2Helper.ComputeQuality(task.RepetitionSession.AverageActionTime, task.ActionTimeSpan, task.ActionCounter, similarity);

                    entryIdToQuality.Add(entry.Id, quality);

                    if (SM2Helper.IsPassingQuality(quality))
                        correctTaskCounter++;
                }
                else
                {
                    missedEntriesCounter++;
                }
            }


            await _stateService.SetQualityRepetitionStateAsync(userId, entryIdToQuality);

            RepetitionResult result = new RepetitionResult(correctTaskCounter, existEntries);
            result.StartedAt = session.StartedAt;
            result.FinishedAt = session.FinishedAt.Value;

            _context.RepetitionResults.Add(result);
            _context.RepetitionSessions.Remove(session);
            _context.RepetitionTasks.RemoveRange(tasks);
            await _context.SaveChangesAsync();

            return RequestResult<RepetitionResult>.Success(result);
        }

        public async Task<RequestResult<RepetitionTask>> SubmitRepetitionTaskAnswerAsync(int userId, int taskId, string answer)
        {
            var task = await _sessionQueries.GetTaskByIdAsync(userId, taskId);

            if (task == null)
                return RequestResult<RepetitionTask>.Failure(ErrorCode.TaskNotFound);


            var currentTime = DateTime.UtcNow;
            var lastActionTime = task.RepetitionSession.LastActionAt;

            task.ActionCounter++;
            task.UserAnswer = answer;
            task.ActionTimeSpan = currentTime - lastActionTime;
            task.RepetitionSession.LastActionAt = currentTime;

            await _context.SaveChangesAsync();

            return RequestResult<RepetitionTask>.Success(task);
        }
    }
}
