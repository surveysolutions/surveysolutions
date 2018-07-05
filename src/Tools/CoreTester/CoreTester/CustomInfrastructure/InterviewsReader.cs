using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage;

namespace CoreTester.CustomInfrastructure
{
    public class InterviewsReader
    {
        private readonly ILogger logger;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly INativeReadSideStorage<InterviewSummary> interviewSummaries;
        private readonly IEventStore eventStore;

        public InterviewsReader(ILogger logger,
            ITransactionManagerProvider transactionManager,
            INativeReadSideStorage<InterviewSummary> interviewSummaries, 
            IEventStore eventStore)
        {
            this.logger = logger;
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.eventStore = eventStore;
        }

        private List<Guid> interviewIds;
        private readonly List<Guid> interviewWithCalculationError = new List<Guid>();
        private Stopwatch interviewsProcessingStopwatch;
        private int lastTimeSomethingWasDumpedInOutput = 0;
        private int dotsInARow = 0;
        private int interviewsProcessed = 0;
        private int pingIntervalInMinutes = 2;
        private string questionnaireRepositoryId;
        private string questionnaireTitle;
        private int eventsCount = 0;

        public InterviewsReader LoadInterviewsListForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaire)
        {
            var stopwatch = Stopwatch.StartNew();
            questionnaireRepositoryId = $"{questionnaireIdentity.QuestionnaireId.FormatGuid()}${questionnaireIdentity.Version}";
            questionnaireTitle = questionnaire.Title;
            interviewIds = GetCompletedInterviewIdsByQuestionnaire(questionnaireIdentity).ToList();

            stopwatch.Stop();

            this.logger.Info($"Received {interviewIds.Count:N0} interviewIds to process. " +
                             $"Questionnaire {questionnaireIdentity}. " +
                             $"Took {stopwatch.Elapsed:g} to complete.");

            Console.WriteLine($"{interviewIds.Count} interviews were found.");
            return this;
        }

        public InterviewsReader Initialize()
        {
            interviewsProcessingStopwatch = Stopwatch.StartNew();
            return this;
        }

        public InterviewsReader ForEachInterview(Action<Guid, InterviewsReader, List<CommittedEvent>> action)
        {
            foreach (var interviewId in interviewIds)
            {
                if (interviewWithCalculationError.Count > 10)
                {
                    Console.WriteLine("10 Interviewes with calculation errors found. Finishing task.");
                    break;
                }

                interviewsProcessed++;

                List<CommittedEvent> events = this.eventStore.Read(interviewId, 0).ToList();

                action(interviewId, this, events);

                eventsCount += events.Count;

                ReportProgress();
            }
            return this;
        }

        public InterviewsReader DumpResultsInFile(string fileName)
        {
            if (interviewWithCalculationError.Count <= 0) 
                return this;

            Console.WriteLine("Dumping debug information");
                
            if (File.Exists(fileName))
                File.Delete(fileName);
                    
            File.AppendAllLines(fileName, new string[]
            {
                "============================================",
                $"=Questionnaire: {questionnaireRepositoryId}",
                $"=               {questionnaireTitle}",
                "=Interviews with calculation error: "
            });
            File.AppendAllLines(fileName, interviewWithCalculationError.Select(x => x.ToString()));
            File.AppendAllLines(fileName,new string[]
            {
                "============================================",
            });
            return this;
        }

        public InterviewsReader Finish()
        {
            interviewsProcessingStopwatch.Stop();
            ShowStatistics(interviewsProcessingStopwatch.ElapsedMilliseconds, interviewIds.Count, eventsCount, interviewIds.Count);
            return this;
        }


        public void AddInterviewWithError(Guid interviewId)
        {
            this.interviewWithCalculationError.Add(interviewId);
        }

        public void ReportProgress()
        {
            if (!(interviewsProcessingStopwatch.Elapsed.TotalMinutes > lastTimeSomethingWasDumpedInOutput + pingIntervalInMinutes)) 
                return;

            lastTimeSomethingWasDumpedInOutput = (int) interviewsProcessingStopwatch.Elapsed.TotalMinutes;
            Console.Write('.');
            dotsInARow++;

            if (dotsInARow % 10 != 0) 
                return;

            ShowStatistics(interviewsProcessingStopwatch.ElapsedMilliseconds, interviewsProcessed, eventsCount, interviewIds.Count);
            dotsInARow = 0;
        }

        private IEnumerable<Guid> GetCompletedInterviewIdsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            var skipInterviewsCount = 0;
            List<Guid> batchInterviews;

            do
            {
                var questionnaireIdentityAsString = questionnaireIdentity.ToString();
                batchInterviews = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ => _
                        .Where(x => x.QuestionnaireIdentity == questionnaireIdentityAsString && x.WasCompleted)
                        .Select(x => x.InterviewId)
                        .Skip(skipInterviewsCount)
                        .Take(1000)
                        .ToList()));

                skipInterviewsCount += batchInterviews.Count;
                this.logger.Debug($"Received {skipInterviewsCount:n0} interview ids.");

                foreach (var interviewId in batchInterviews)
                    yield return interviewId;
            } while (batchInterviews.Count > 0);
        }

        private static void ShowStatistics(long elapsedMilliseconds, int processedInterviewsCount, int eventsCount,
            int totalInterviewsCount)
        {
            var total = TimeSpan.FromMilliseconds(elapsedMilliseconds);
            var averagePerInterview = TimeSpan.FromMilliseconds(elapsedMilliseconds / processedInterviewsCount);
            var averagePerEvent = TimeSpan.FromMilliseconds(elapsedMilliseconds / eventsCount);
            var finishTime = DateTime.Now.AddMilliseconds((totalInterviewsCount - processedInterviewsCount) *
                                                          (elapsedMilliseconds / processedInterviewsCount));

            Console.WriteLine("--------------------------------------------");
            Console.WriteLine(
                $"{processedInterviewsCount * 100 / totalInterviewsCount:0.##}% finished. Will finish at {finishTime}. {total:g} - time running.");
            Console.WriteLine($"{averagePerInterview:g} - average per interview.");
            Console.WriteLine($"{averagePerEvent:g} - average per event.");
            Console.WriteLine();
        }
    }
}
