using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Infrastructure.Native.Threading;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class InterviewImportService : IInterviewImportService
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IInterviewUniqueKeyGenerator interviewKeyGenerator;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly SampleImportSettings sampleImportSettings;
        private readonly IInterviewImportDataParsingService interviewImportDataParsingService;
        private readonly IInterviewTreeBuilder interviewTreeBuilder;

        private readonly object lockStart = new object();

        private IPlainTransactionManager PlainTransactionManager => plainTransactionManagerProvider.GetPlainTransactionManager();
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IPlainStorageAccessor<Assignment> assignmentPlainStorageAccessor;
        private readonly IUserViewFactory userViewFactory;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IPreloadedDataVerifier preloadedDataVerifier;

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
            IUserViewFactory userViewFactory, 
            IInterviewTreeBuilder interviewTreeBuilder, 
            IPreloadedDataRepository preloadedDataRepository, 
            IPreloadedDataVerifier preloadedDataVerifier)
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
            this.interviewTreeBuilder = interviewTreeBuilder;
            this.preloadedDataRepository = preloadedDataRepository;
            this.preloadedDataVerifier = preloadedDataVerifier;
        }

        public void VerifyAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, string fileName)
        {
            if (StartImportProcess(questionnaireIdentity, interviewImportProcessId, AssignmentImportType.Assignments)) return;

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            this.Status.Stage = AssignmentImportStage.FileVerification;
            this.Status.VerificationState.FileName = fileName;

            PreloadedDataByFile[] preloadedPanelData = this.preloadedDataRepository.GetPreloadedDataOfPanel(interviewImportProcessId);

            this.preloadedDataVerifier.VerifyPanelFiles(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, preloadedPanelData, this.Status);

            void VerifyAssignmentAction(AssignmentImportData assignmentRecord)
            {
                var result = VerifyAssignment(assignmentRecord.PreloadedData.Answers.GroupedByLevels(), questionnaire);
                if (!result.Status)
                    throw new InterviewException(result.ErrorMessage);
            }

            if (this.Status.VerificationState.Errors.Any())
            {
                FinishImportProcess();
                return;
            }

            this.Status.ProcessedCount = 0;
            this.Status.TotalCount = this.Status.VerificationState.EntitiesCount;
            this.Status.Stage = AssignmentImportStage.AssignmentDataVerification;

            AssignmentImportData[] assignmentImportData = this.interviewImportDataParsingService.GetAssignmentsData(interviewImportProcessId, questionnaireIdentity, AssignmentImportType.Panel);
            RunImportProcess(assignmentImportData, questionnaireIdentity, VerifyAssignmentAction);
        }
        public AssignmentVerificationResult VerifyAssignment(List<InterviewAnswer>[] answersGroupedByLevels, IQuestionnaire questionnaire)
        {
            try
            {
                var tree = this.interviewTreeBuilder.BuildInterviewTree(Guid.NewGuid(), questionnaire);

                var noAnswersOnQuestionnaireLevel =
                    answersGroupedByLevels.All(x => x.FirstOrDefault()?.Identity.RosterVector.Length != 0);
                if (noAnswersOnQuestionnaireLevel)
                    tree.ActualizeTree();

                foreach (var answersInLevel in answersGroupedByLevels)
                {
                    foreach (InterviewAnswer answer in answersInLevel)
                    {
                        var interviewTreeQuestion = tree.GetQuestion(answer.Identity);
                        if (interviewTreeQuestion == null)
                            continue;

                        interviewTreeQuestion.SetAnswer(answer.Answer);

                        interviewTreeQuestion.RunImportInvariantsOrThrow(new InterviewQuestionInvariants(answer.Identity, questionnaire, tree));
                    }
                    tree.ActualizeTree();
                }
            }
            catch (Exception e)
            {
                return AssignmentVerificationResult.Error(e.Message);
            }

            return AssignmentVerificationResult.Ok();
        }

        public void ImportAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, Guid? responsibleId, Guid headquartersId, 
            AssignmentImportType mode, bool shouldSkipInterviewCreation)
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

                List<InterviewAnswer> answers = assignmentRecord.PreloadedData.Answers;
                var assignment = new Assignment(questionnaireIdentity, assignmentResponsibleId, assignmentRecord.Quantity);

                List<IdentifyingAnswer> identifyingAnswers = answers.Where(x => identifyingQuestionIds.Contains(x.Identity.Id)).Select(a => IdentifyingAnswer.Create(assignment, questionnaire, a.Answer.ToString(), a.Identity)).ToList();
                assignment.SetIdentifyingData(identifyingAnswers);
                assignment.SetAnswers(answers);

                // need save an assignment first to get a real assignmentId
                this.PlainTransactionManager.ExecuteInPlainTransaction(() => this.assignmentPlainStorageAccessor.Store(assignment, null));

                if (!shouldSkipInterviewCreation)
                {
                    this.transactionManagerProvider.GetTransactionManager().ExecuteInQueryTransaction(() => this.PlainTransactionManager.ExecuteInPlainTransaction(() => this.commandService.Execute(new CreateInterview(Guid.NewGuid(), headquartersId, questionnaireIdentity, supervisorId: responsibleSupervisorId, interviewerId: responsibleInterviewerId, answersTime: DateTime.UtcNow, answers: answers, interviewKey: this.interviewKeyGenerator.Get(), assignmentId: assignment.Id))));
                }
            }

            if (StartImportProcess(questionnaireIdentity, interviewImportProcessId, AssignmentImportType.Assignments)) return;
            this.Status.Stage = AssignmentImportStage.AssignmentCreation;
            RunImportProcess(assignmentImportData, questionnaireIdentity, ImportAction);
        }

        private void RunImportProcess(AssignmentImportData[] records, QuestionnaireIdentity questionnaireIdentity, Action<AssignmentImportData> importAction)
        {
            try
            {
                var questionnaireDocument = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
                this.Status.QuestionnaireTitle = questionnaireDocument.Title;

                if (records == null)
                {
                    this.Status.State.Errors.Add(new InterviewImportError
                    {
                        ErrorMessage = Interviews.ImportInterviews_IncorrectDatafile
                    });

                    return;
                }

                this.Status.TotalCount = records.Length;

                int createdInterviewsCount = 0;
                Stopwatch elapsedTime = Stopwatch.StartNew();

                Parallel.ForEach(records,
                    new ParallelOptions { MaxDegreeOfParallelism = this.sampleImportSettings.InterviewsImportParallelTasksLimit },
                    record =>
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

                        this.Status.ProcessedCount = Interlocked.Increment(ref createdInterviewsCount);
                        this.Status.ElapsedTime = elapsedTime.ElapsedMilliseconds;
                        this.Status.TimePerItem = this.Status.ElapsedTime / this.Status.ProcessedCount;
                        this.Status.EstimatedTime = this.Status.TimePerItem * this.Status.TotalCount;
                    });

                this.logger.Info($"Imported {this.Status.TotalCount:N0} of interviews. Took {elapsedTime.Elapsed:c} to complete");
            }
            finally
            {
                FinishImportProcess();
            }
        }

        private bool StartImportProcess(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId,
            AssignmentImportType assignmentImportType)
        {
            lock (lockStart)
            {
                if (this.Status.IsInProgress)
                    return true;

                this.Status = new AssignmentImportStatus
                {
                    QuestionnaireId = questionnaireIdentity,
                    InterviewImportProcessId = interviewImportProcessId,
                    StartedDateTime = DateTime.Now,
                    ProcessedCount = 0,
                    ElapsedTime = 0,
                    EstimatedTime = 0,
                    State = {Columns = new string[0], Errors = new List<InterviewImportError>()},
                    IsInProgress = true,
                    AssignmentImportType = assignmentImportType
                };
            }
            return false;
        }

        private void FinishImportProcess()
        {
            this.Status.IsInProgress = false;
        }

        private string FormatInterviewImportData(InterviewImportData importedInterview)
        {
            return string.Join(", ", importedInterview.PreloadedData.Data[0].Answers.Values.Where(x => x != null));
        }
        
        public AssignmentImportStatus Status { get; private set; } = new AssignmentImportStatus();

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
            return this.PlainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                var user = this.userViewFactory.GetUser(new UserViewInputModel(userId));
                if (user == null || user.IsArchived)
                    return null;
                return user;
            });
        }
    }
}