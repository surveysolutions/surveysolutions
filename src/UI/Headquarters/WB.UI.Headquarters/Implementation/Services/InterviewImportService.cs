using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
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
        private readonly IUserViewFactory userViewFactory;
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
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IUserViewFactory userViewFactory)
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
            this.userViewFactory = userViewFactory;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
        }

        public void ImportAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, Guid? responsibleId, Guid headquartersId, PreloadedContentType mode)
        {
            AssignmentImportData[] assignmentImportData = this.interviewImportDataParsingService.GetAssignmentsData(interviewImportProcessId, questionnaireIdentity, mode);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
            var identifyingQuestionIds = questionnaire.GetPrefilledQuestions().ToHashSet();

            GetInterviewerAndSupervisorIdsByResponsibleId(responsibleId, out Guid? supervisorIdByResponsible, out Guid? interviewerIdByResponsible);

            void ImportAction(AssignmentImportData assignmentRecord)
            {
                if (!responsibleId.HasValue && !assignmentRecord.SupervisorId.HasValue)
                {
                    this.Status.State.Errors.Add(new InterviewImportError
                    {
                        ErrorMessage = string.Format(Interviews.ImportInterviews_FailedToImportInterview_NoSupervisor, this.FormatInterviewImportData(assignmentRecord))
                    });
                    return;
                }

                var responsibleSupervisorId = assignmentRecord.SupervisorId ?? supervisorIdByResponsible.Value;
                var responsibleInterviewerId = assignmentRecord.SupervisorId.HasValue ? assignmentRecord.InterviewerId : interviewerIdByResponsible;
                var assignmentResponsibleId = responsibleInterviewerId ?? responsibleSupervisorId;

                var questionnaireBrowseItem = this.plainTransactionManager.ExecuteInQueryTransaction(() => this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity));

                List<InterviewAnswer> answers = assignmentRecord.PreloadedData.Answers;
                var assignment = new Assignment(questionnaireIdentity, assignmentResponsibleId, assignmentRecord.Quantity);

                List<IdentifyingAnswer> identifyingAnswers = answers.Where(x => identifyingQuestionIds.Contains(x.Identity.Id)).Select(a => IdentifyingAnswer.Create(assignment, questionnaire, a.Answer.ToString(), a.Identity)).ToList();
                assignment.SetIdentifyingData(identifyingAnswers);
                assignment.SetAnswers(answers);

                // need save assignment firstly for get real assignmentId
                this.plainTransactionManager.ExecuteInPlainTransaction(() => this.assignmentPlainStorageAccessor.Store(assignment, null));

                bool isSupportAssignments = questionnaireBrowseItem.AllowAssignments;
                if (!isSupportAssignments)
                {
                    this.transactionManagerProvider.GetTransactionManager().ExecuteInQueryTransaction(() => this.plainTransactionManager.ExecuteInPlainTransaction(() => this.commandService.Execute(new CreateInterviewWithPreloadedData(Guid.NewGuid(), headquartersId, questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, supervisorId: responsibleSupervisorId, interviewerId: responsibleInterviewerId, answersTime: DateTime.UtcNow, answers: answers, interviewKey: this.interviewKeyGenerator.Get(), assignmentId: assignment.Id))));
                }
            }

            RunImportProcess(assignmentImportData, questionnaireIdentity, interviewImportProcessId, PreloadedContentType.Assignments, ImportAction);
        }

        private void RunImportProcess(AssignmentImportData[] records, 
            QuestionnaireIdentity questionnaireIdentity, 
            string interviewImportProcessId, 
            PreloadedContentType preloadedContentType, 
            Action<AssignmentImportData> importAction)
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

        private void GetInterviewerAndSupervisorIdsByResponsibleId(Guid? responsibleId, 
            out Guid? responsibleSupervisorId, out Guid? responsibleInterviewerId)
        {
            if (!responsibleId.HasValue)
            {
                responsibleSupervisorId = null;
                responsibleInterviewerId = null;
                return;
            }

            var responsible = this.GetUserById(responsibleId.Value);
            if (responsible.IsInterviewer())
            {
                responsibleSupervisorId = responsible.Supervisor.Id;
                responsibleInterviewerId = responsible.PublicKey;
            }
            else
            {
                responsibleSupervisorId = responsible.PublicKey;
                responsibleInterviewerId = null;
            }
        }

        private UserView GetUserById(Guid userId)
        {
            return this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                var user = this.userViewFactory.GetUser(new UserViewInputModel(userId));
                if (user == null || user.IsArchived)
                    return null;
                return user;
            });
        }
    }
}