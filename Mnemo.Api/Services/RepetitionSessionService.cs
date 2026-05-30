using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mnemo.Contracts.Dtos.Repetition;
using Mnemo.Data;
using Mnemo.Common;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;

namespace Mnemo.Services
{
    public class RepetitionSessionService
    {
        private AppDbContext _context;
        private AccountQueries _accountQueries;
        private SessionQueries _sessionQueries;
        private VocabularyQueries _vocabularyQueries;

        private RepetitionStateService _stateService;

        private static Random _random = new Random();


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

            if (session == null)    return RequestResult<RepetitionSession>.Failure(ErrorCode.SessionNotFound);
            else return RequestResult<RepetitionSession>.Failure(ErrorCode.SessionNotFinished);
        }



        public async Task<RequestResult<RepetitionSession>> StartRepetitionSessionAsync(int userId, string mode)
        {
            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<RepetitionSession>.Failure(ErrorCode.UserNotFound);

            if (await _sessionQueries.ExistsByUserId(userId))
                return RequestResult<RepetitionSession>.Failure(ErrorCode.SessionNotFinished);


            await _stateService.CreateNonExistentRepetitionStatesAsync(userId);


            var targetEntries = mode switch
            {
                "fast" => _vocabularyQueries.GetRandomByUserId(userId, 10),
                "planned"  => _vocabularyQueries.GetDueByUserIdAsync(userId),
                _ => new List<VocabularyEntry>()
            };

            if (!targetEntries.Any())
                return RequestResult<RepetitionSession>.Failure(ErrorCode.TaskNotFound);


            var tasks = new List<RepetitionTask>();
            foreach (var target in targetEntries)
            {
                bool isForwardQuestion = _random.Next(2) == 0;
                bool withOptions = _random.Next(2) == 0;

                string prompt, answer;
                if (isForwardQuestion)
                {
                    answer = target.Translations[0];
                    prompt = target.Foreign;
                }
                else
                {
                    answer = target.Foreign;
                    prompt = target.Translations[0];
                }

                var options = new List<string>();
                if(withOptions)
                {
                    var otherEntries = _vocabularyQueries.GetRandomByUserId(userId, 3, target.Id);

                    foreach (var entry in otherEntries)
                    {
                        string option = isForwardQuestion ? entry.Translations[0] : entry.Foreign;
                        if (!options.Contains(option) && option != answer)
                            options.Add(option);
                    }

                    options.Add(answer);
                    options = options.OrderBy(x => Guid.NewGuid()).ToList();
                }

                tasks.Add(new RepetitionTask(target.Id, isForwardQuestion, prompt, options));
            }

            var session = new RepetitionSession(userId, tasks, mode == "planned" ? true : false);

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

            if (tasks.Count == 0)
                return RequestResult<RepetitionResult>.Failure(ErrorCode.TaskNotFound);

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


            if (session.IsPlanned)
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


            var currentTime     = DateTime.UtcNow;
            var lastActionTime  = task.RepetitionSession.LastActionAt;

            task.ActionCounter++;
            task.UserAnswer             = answer;
            task.ActionTimeSpan         = currentTime - lastActionTime;
            task.RepetitionSession.LastActionAt = currentTime;

            await _context.SaveChangesAsync();

            return RequestResult<RepetitionTask>.Success(task);
        }
    }
}
