using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Threading;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Implementation.Services
{
    public class InterviewImportService : IInterviewImportService
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IInterviewUniqueKeyGenerator interviewKeyGenerator;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly SampleImportSettings sampleImportSettings;
        private readonly IInterviewImportDataParsingService interviewImportDataParsingService;

        private readonly object lockStart = new object();

        private IPlainTransactionManager plainTransactionManager => plainTransactionManagerProvider.GetPlainTransactionManager();
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IPlainStorageAccessor<Assignment> assignmentPlainStorageAccessor;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;

        public InterviewImportService(
            ICommandService commandService,
            ILogger logger,
            SampleImportSettings sampleImportSettings, 
            IInterviewImportDataParsingService interviewImportDataParsingService, 
            IQuestionnaireStorage questionnaireStorage,
            IInterviewUniqueKeyGenerator interviewKeyGenerator,
            IPlainTransactionManagerProvider plainTransactionManagerProvider,
            ITransactionManagerProvider transactionManagerProvider,
            IPlainStorageAccessor<Assignment> assignmentPlainStorageAccessor,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory)
        {
            this.commandService = commandService;
            this.logger = logger;
            this.sampleImportSettings = sampleImportSettings;
            this.interviewImportDataParsingService = interviewImportDataParsingService;
            this.questionnaireStorage = questionnaireStorage;
            this.interviewKeyGenerator = interviewKeyGenerator;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.transactionManagerProvider = transactionManagerProvider;
            this.assignmentPlainStorageAccessor = assignmentPlainStorageAccessor;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
        }

        public void ImportAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, Guid? supervisorId, Guid headquartersId)
        {
            var assignmentImportData = this.interviewImportDataParsingService.GetAssignmentsImportDataForSample(interviewImportProcessId, questionnaireIdentity);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
            RunImportProcess(assignmentImportData,
                questionnaireIdentity,
                interviewImportProcessId,
                PreloadedContentType.Assignments, 
                (assignmentRecord) =>
                {
                    if (!supervisorId.HasValue && !assignmentRecord.SupervisorId.HasValue)
                    {
                        this.Status.State.Errors.Add(new InterviewImportError
                        {
                            ErrorMessage = string.Format(Interviews.ImportInterviews_FailedToImportInterview_NoSupervisor,
                                    this.FormatInterviewImportData(assignmentRecord))
                        });
                        return;
                    }

                    var responsibleSupervisorId = assignmentRecord.SupervisorId ?? supervisorId.Value;

                    var questionnaireBrowseItem = this.plainTransactionManager.ExecuteInQueryTransaction(
                        () => this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity));

                    var responsibleId = assignmentRecord.InterviewerId ?? responsibleSupervisorId;
                    var topLevelAnswers = assignmentRecord.PreloadedData.Data.First();
                    var assignment = new Assignment(questionnaireIdentity, 
                        responsibleId,
                        assignmentRecord.Quantity);


                    var identifyingAnswers = topLevelAnswers.Answers.Select(a => new IdentifyingAnswer(assignment, questionnaire, a.Value.ToString(), a.Key)).ToList();
                    assignment.SetAnswers(identifyingAnswers);

                    this.plainTransactionManager.ExecuteInPlainTransaction(
                        () => assignmentPlainStorageAccessor.Store(assignment, null));

                    bool isSupportAssignments = questionnaireBrowseItem.AllowAssignments;
                    if (!isSupportAssignments)
                    {
                        this.transactionManagerProvider.GetTransactionManager().ExecuteInQueryTransaction(() =>
                            this.plainTransactionManager.ExecuteInPlainTransaction(
                                () => this.commandService.Execute(
                                    new CreateInterviewWithPreloadedData(
                                        Guid.NewGuid(),
                                        headquartersId,
                                        questionnaireIdentity.QuestionnaireId,
                                        questionnaireIdentity.Version,
                                        supervisorId: responsibleSupervisorId,
                                        interviewerId: assignmentRecord.InterviewerId,
                                        answersTime: DateTime.UtcNow,
                                        preloadedDataDto: assignmentRecord.PreloadedData,
                                        interviewKey: this.interviewKeyGenerator.Get(),
                                        assignmentId: assignment.Id))));
                    }
                });
        }

        public void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId,
            Guid? supervisorId, Guid headquartersId)
        {
            var interviewsToImport = this.interviewImportDataParsingService.GetInterviewsImportDataForPanel(interviewImportProcessId, questionnaireIdentity);

            RunImportProcess(interviewsToImport,
                questionnaireIdentity,
                interviewImportProcessId,
                PreloadedContentType.Panel, 
                (importedInterview) =>
                {
                if (!supervisorId.HasValue && !importedInterview.SupervisorId.HasValue)
                    {
                        this.Status.State.Errors.Add(new InterviewImportError
                        {
                            ErrorMessage =
                                string.Format(Interviews.ImportInterviews_FailedToImportInterview_NoSupervisor, this.FormatInterviewImportData(importedInterview))
                        });
                        return;
                    }
                    var responsibleSupervisorId = importedInterview.SupervisorId ?? supervisorId.Value;

                    this.transactionManagerProvider.GetTransactionManager().ExecuteInQueryTransaction(() =>
                            this.plainTransactionManager.ExecuteInPlainTransaction(
                                () => this.commandService.Execute(
                                    new CreateInterviewWithPreloadedData(
                                        Guid.NewGuid(),
                                        headquartersId,
                                        questionnaireIdentity.QuestionnaireId,
                                        questionnaireIdentity.Version,
                                        supervisorId: responsibleSupervisorId,
                                        interviewerId: importedInterview.InterviewerId,
                                        answersTime: DateTime.UtcNow,
                                        preloadedDataDto: importedInterview.PreloadedData,
                                        interviewKey: this.interviewKeyGenerator.Get(),
                                        assignmentId: null))));
                });
        }

        private void RunImportProcess<T>(T[] records, 
            QuestionnaireIdentity questionnaireIdentity, 
            string interviewImportProcessId, 
            PreloadedContentType preloadedContentType, 
            Action<T> importAction)
            where T : InterviewImportData
        {
            lock (lockStart)
            {
                if (this.Status.IsInProgress)
                    return;

                this.Status = new InterviewImportStatus
                {
                    QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                    InterviewImportProcessId = interviewImportProcessId,
                    QuestionnaireVersion = questionnaireIdentity.Version,
                    StartedDateTime = DateTime.Now,
                    CreatedInterviewsCount = 0,
                    ElapsedTime = 0,
                    EstimatedTime = 0,
                    State = { Columns = new string[0], Errors = new List<InterviewImportError>() },
                    IsInProgress = true,
                    PreloadedContentType = preloadedContentType
                };
            }

            try
            {
                var questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
                this.Status.QuestionnaireTitle = questionnaireDocument.Title;

                if (records == null)
                {
                    this.Status.State.Errors.Add(new InterviewImportError
                    {
                        ErrorMessage = Interviews.ImportInterviews_IncorrectDatafile
                    });

                    return;
                }

                this.Status.TotalInterviewsCount = records.Length;

                int createdInterviewsCount = 0;
                Stopwatch elapsedTime = Stopwatch.StartNew();

                Parallel.ForEach(records,
                    new ParallelOptions { MaxDegreeOfParallelism = this.sampleImportSettings.InterviewsImportParallelTasksLimit },
                    (record) =>
                    {
                        try
                        {
                            ThreadMarkerManager.MarkCurrentThreadAsIsolated();

                            importAction.Invoke(record);
                        }
                        catch (Exception ex)
                        {
                            var errorMessage = string.Format(Interviews.ImportInterviews_GenericError, this.FormatInterviewImportData(record), record.InterviewerId, questionnaireIdentity, ex.Message);

                            this.logger.Error(errorMessage, ex);

                            this.Status.State.Errors.Add(new InterviewImportError { ErrorMessage = errorMessage });
                        }
                        finally
                        {
                            ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                        }

                        this.Status.CreatedInterviewsCount = Interlocked.Increment(ref createdInterviewsCount);
                        this.Status.ElapsedTime = elapsedTime.ElapsedMilliseconds;
                        this.Status.TimePerInterview = this.Status.ElapsedTime / this.Status.CreatedInterviewsCount;
                        this.Status.EstimatedTime = this.Status.TimePerInterview * this.Status.TotalInterviewsCount;
                    });

                this.logger.Info($"Imported {this.Status.TotalInterviewsCount:N0} of interviews. Took {elapsedTime.Elapsed:c} to complete");
            }
            finally
            {
                this.Status.IsInProgress = false;
            }
        }

        private string FormatInterviewImportData(InterviewImportData importedInterview)
        {
            return string.Join(", ", importedInterview.PreloadedData.Data[0].Answers.Values.Where(x => x != null));
        }
        
        public InterviewImportStatus Status { get; private set; } = new InterviewImportStatus();
    }
}