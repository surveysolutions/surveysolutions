using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using Utils.CustomInfrastructure;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage;

namespace Utils.Commands
{
    public class CoreTestRunner
    {
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IPlainTransactionManagerProvider plainTransactionManager;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireBrowseViewFactory questionnairesBrowseFactory;
        private readonly INativeReadSideStorage<InterviewSummary> interviewSummaries;
        private readonly IEventStore eventStore;
        private IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;

        public CoreTestRunner(
            ICommandService commandService,
            IQuestionnaireBrowseViewFactory questionnairesBrowseFactory,
            ILogger logger,
            ITransactionManagerProvider transactionManager,
            INativeReadSideStorage<InterviewSummary> interviewSummaries,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireRepository,
            IEventStore eventStore,
            IQuestionnaireStorage questionnaireStorage, 
            IPlainTransactionManagerProvider plainTransactionManager,
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor)
        {
            this.commandService = commandService;
            this.questionnairesBrowseFactory = questionnairesBrowseFactory;
            this.logger = logger;
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.questionnaireStorage = questionnaireStorage;
            this.plainTransactionManager = plainTransactionManager;
            this.eventStore = eventStore;
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
        }

        public int Run(string serverName)
        {
            logger.Info($"TestRunner for db {serverName}");

            var questionnaireBrowseItems = this.plainTransactionManager.GetPlainTransactionManager()
                .ExecuteInQueryTransaction(() =>
                    questionnairesBrowseFactory.Load(new QuestionnaireBrowseInputModel {PageSize = 1000}));
            

            Console.WriteLine($"Found {questionnaireBrowseItems.Items.Count()} questionnaires");

            foreach (var questionnaireBrowseItem in questionnaireBrowseItems.Items)
            {
                var questionnaireIdentity = new QuestionnaireIdentity(questionnaireBrowseItem.QuestionnaireId,
                    questionnaireBrowseItem.Version);
                var interviewIdsToProcess = GetCompletedInterviewIdsByQuestionnaire(questionnaireIdentity).ToList();
                if (interviewIdsToProcess.Count <= 0) continue;

                var questionnaireRepositoryId =
                    $"{questionnaireBrowseItem.QuestionnaireId.FormatGuid()}${questionnaireBrowseItem.Version}";
                QuestionnaireDocument questionnaire = this.plainTransactionManager.GetPlainTransactionManager()
                    .ExecuteInQueryTransaction(() =>
                        questionnaireRepository.GetById(questionnaireRepositoryId));

                Console.WriteLine("============================================");
                Console.WriteLine($"Questionnaire: {questionnaireRepositoryId}");
                Console.WriteLine($"               {questionnaire.Title}");
                Console.WriteLine($"{interviewIdsToProcess.Count} interviews were found.");

                var isExistsMacrosesInDocument = Utils.IsExistsMacrosesInDocument(questionnaire);
                if (isExistsMacrosesInDocument)
                {
                    var assemblyFileName = Path.Combine(Path.GetTempPath(), $"assembly-{questionnaireIdentity}.dll");
                    var assemblyAsBytes = this.plainTransactionManager.GetPlainTransactionManager()
                        .ExecuteInQueryTransaction(() => questionnaireAssemblyFileAccessor.GetAssemblyAsByteArray(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

                    if (File.Exists(assemblyFileName))
                        File.Delete(assemblyFileName);

                    File.WriteAllBytes(assemblyFileName, assemblyAsBytes);

                    if (Utils.IsSupportedDecompile(assemblyFileName))
                    {
                        Utils.InlineMacrosesInDocument(questionnaire, assemblyFileName);
                    }
                    else
                    {
                        Console.WriteLine($"Dll doesn't supported decompile operation. Questionnaire: {questionnaireRepositoryId} skiped.");
                        continue;
                    }
                }

                questionnaireStorage.StoreQuestionnaire(questionnaireBrowseItem.QuestionnaireId, questionnaireBrowseItem.Version, questionnaire);
                var stopwatch = Stopwatch.StartNew();
                var eventsCount = 0;
                int lastTimeSomethingWasDumpedInOutput = 0;
                int pingIntervalInMinutes = 2;
                int dotsInARow = 0;
                int interviewsProcessed = 0;
                var interviewWithCalculationError = new List<Guid>();

                
                foreach (var interviewId in interviewIdsToProcess)
                {
                    if (interviewWithCalculationError.Count > 10)
                    {
                        Console.WriteLine("10 Interviewes with calculation errors found. Finishing task.");
                        break;
                    }

                    interviewsProcessed++;
                    var newInterviewId = interviewId; //  Guid.Parse("11111111111111111111111111111111");
                    var userId = Guid.Parse("22222222222222222222222222222222");

                    var committedEvents = this.eventStore.Read(interviewId, 0).ToList();

                    if (committedEvents.Count == 0)
                        continue;

                    var createCommand = EventsToCommandConverter.GetCreateInterviewCommand(committedEvents, newInterviewId, userId);
                    // to read assembly
                    this.plainTransactionManager.GetPlainTransactionManager()
                        .ExecuteInQueryTransaction(() => commandService.Execute(createCommand));

                    var indexOfFirstSupervisorAssignedEvent = committedEvents.FindIndex(0, x => x.Payload is SupervisorAssigned);

                    foreach (var committedEvent in committedEvents.Skip(indexOfFirstSupervisorAssignedEvent))
                    {
                        var commands = EventsToCommandConverter.ConvertEventToCommands(newInterviewId, committedEvent);

                        if (commands == null)
                            continue;

                        try
                        {
                            foreach (var command in commands)
                            {
                                if (command is RemoveAnswerCommand)
                                {
                                    try
                                    {
                                        commandService.Execute(command);
                                    }
                                    catch (InterviewException exception)
                                    {
                                        if (!(exception.Message.Contains(
                                                  "is disabled and question's answer cannot be changed") ||
                                              exception.Message.Contains("No questions found for roster vector")))
                                        {
                                            throw;
                                        }
                                    }
                                }
                                else
                                {
                                    commandService.Execute(command);
                                }
                            }
                        }
                        catch (InterviewException exception)
                        {
                            var message = exception.ExceptionType ==
                                          InterviewDomainExceptionType.ExpessionCalculationError
                                ? $"Calculation error! IN: {interviewId}. Event: {committedEvent.EventSequence} / {committedEvents.Count}"
                                : $"General error! Exception type:{exception.ExceptionType} IN: {interviewId}. Event: {committedEvent.EventSequence} / {committedEvents.Count}";

                            if (exception.ExceptionType == InterviewDomainExceptionType.ExpessionCalculationError)
                            {
                                interviewWithCalculationError.Add(interviewId);
                                Console.WriteLine(message);
                            }

                            this.logger.Info(message, exception);
                            break;
                        }

                        if (!(stopwatch.Elapsed.TotalMinutes >
                              lastTimeSomethingWasDumpedInOutput + pingIntervalInMinutes)) continue;
                        lastTimeSomethingWasDumpedInOutput = (int) stopwatch.Elapsed.TotalMinutes;
                        Console.Write('.');
                        dotsInARow++;
                        if (dotsInARow % 10 == 0)
                        {
                            ShowStatistics(stopwatch.ElapsedMilliseconds, interviewsProcessed, eventsCount,
                                interviewIdsToProcess.Count);
                            dotsInARow = 0;
                        }
                    }

                    commandService.Execute(new DeleteInterviewCommand(newInterviewId, userId));


                    eventsCount += committedEvents.Count;
                }

                if (interviewWithCalculationError.Count > 0)
                {
                    Console.WriteLine("Dumping debug information");

                    var fileName = $"{serverName}.results.txt";
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                    
                    File.AppendAllLines(fileName, new string[]
                    {
                        "============================================",
                        $"=Questionnaire: {questionnaireRepositoryId}",
                        $"=               {questionnaire.Title}",
                        "=Interviews with calculation error: "
                    });
                    File.AppendAllLines(fileName, interviewWithCalculationError.Select(x => x.ToString()));
                    File.AppendAllLines(fileName,new string[]
                    {
                        "============================================",
                    });
                }

                stopwatch.Stop();
                ShowStatistics(stopwatch.ElapsedMilliseconds, interviewIdsToProcess.Count, eventsCount,
                    interviewIdsToProcess.Count);

                questionnaireStorage.DeleteQuestionnaireDocument(questionnaireBrowseItem.QuestionnaireId,
                    questionnaireBrowseItem.Version);
            }

            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"Finished at {DateTime.Now}");
            return 0;
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

        public IEnumerable<Guid> GetCompletedInterviewIdsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            var skipInterviewsCount = 0;
            var batchInterviews = new List<Guid>();

            var stopwatch = Stopwatch.StartNew();

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

            stopwatch.Stop();

            this.logger.Info($"Received {skipInterviewsCount:N0} interviewIds to process. " +
                             $"Questionnaire {questionnaireIdentity.ToString()}. " +
                             $"Took {stopwatch.Elapsed:g} to complete.");
        }
    }
}
