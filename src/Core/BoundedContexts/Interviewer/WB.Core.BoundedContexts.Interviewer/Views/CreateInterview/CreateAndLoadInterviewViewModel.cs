using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.CreateInterview
{
    public class CreateAndLoadInterviewViewModel : ProgressViewModel<CreateInterviewViewModelArg>
    {
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IInterviewerPrincipal interviewerPrincipal;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;
        private readonly ICommandService commandService;
        private readonly ILastCreatedInterviewStorage lastCreatedInterviewStorage;
        private readonly ILogger logger;
        private readonly IAuditLogService auditLogService;
        private readonly IInterviewAnswerSerializer answerSerializer;
        private readonly IUserInteractionService userInteractionService;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public CreateAndLoadInterviewViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IAssignmentDocumentsStorage assignmentsRepository,
            IInterviewerPrincipal interviewerPrincipal,
            IInterviewUniqueKeyGenerator keyGenerator,
            ICommandService commandService,
            ILastCreatedInterviewStorage lastCreatedInterviewStorage,
            ILogger logger,
            IAuditLogService auditLogService,
            IInterviewAnswerSerializer answerSerializer,
            IUserInteractionService userInteractionService,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository) 
            : base(principal, viewModelNavigationService)
        {
            this.assignmentsRepository = assignmentsRepository;
            this.interviewerPrincipal = interviewerPrincipal;
            this.keyGenerator = keyGenerator;
            this.commandService = commandService;
            this.lastCreatedInterviewStorage = lastCreatedInterviewStorage;
            this.logger = logger;
            this.auditLogService = auditLogService;
            this.answerSerializer = answerSerializer;
            this.userInteractionService = userInteractionService;
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
        }

        protected int AssignmentId { get; set; }
        protected Guid InterviewId { get; set; }

        public override void Prepare(CreateInterviewViewModelArg parameter)
        {
            AssignmentId = parameter.AssignmentId;
            InterviewId = parameter.InterviewId;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            if (AssignmentId == 0) throw new ArgumentException(nameof(AssignmentId));
            if (InterviewId == Guid.Empty) throw new ArgumentException(nameof(InterviewId));
            this.ProgressDescription = InterviewerUIResources.Interview_Creating;
            this.OperationDescription = InterviewerUIResources.Interview_Creating_Description;

            var assignmentDocument = assignmentsRepository.GetById(AssignmentId);
            QuestionnaireTitle = assignmentDocument.Title;
        }

        public override void ViewAppeared()
        {
            Task.Run(CreateAndNavigateToInterview);
        }

        private async Task CreateAndNavigateToInterview()
        {
            var interviewId = await CreateInterviewAndNavigateThereAsync(this.AssignmentId, this.InterviewId);
            if (!interviewId.HasValue)
            {
                await this.viewModelNavigationService.NavigateToDashboardAsync();
                return;
            }

            var interview = await LoadInterviewAndNavigateThereAsync(interviewId.Value);
            if (interview == null)
            {
                await this.viewModelNavigationService.NavigateToDashboardAsync();
                return;
            }

            if (interview.HasEditableIdentifyingQuestions)
            {
                await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(interviewId.Value.FormatGuid());
            }
            else
            {
                await this.viewModelNavigationService.NavigateToInterviewAsync(interviewId.Value.FormatGuid(), navigationIdentity: null);
            }
        }

        protected async Task<Guid?> CreateInterviewAndNavigateThereAsync(int assignmentId, Guid interviewId)
        {
            IsIndeterminate = true;
            this.ProgressDescription = InterviewerUIResources.Interview_Creating;
            this.OperationDescription = InterviewerUIResources.Interview_Creating_Description;

            try
            {
                var assignment = this.assignmentsRepository.GetById(assignmentId);

                if (assignment.CreatedInterviewsCount.HasValue && assignment.Quantity.HasValue &&
                    (assignment.CreatedInterviewsCount.Value >= assignment.Quantity.Value))
                {
                    return null;
                }

                var interviewerIdentity = this.interviewerPrincipal.CurrentUserIdentity;

                this.assignmentsRepository.FetchPreloadedData(assignment);
                var questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);

                List<InterviewAnswer> answers = this.GetAnswers(assignment.Answers);
                List<string> protectedVariables = assignment.ProtectedVariables.Select(x => x.Variable).ToList();

                var interviewKey = keyGenerator.Get();
                ICommand createInterviewCommand = new SharedKernels.DataCollection.Commands.Interview.CreateInterview(interviewId,
                    interviewerIdentity.UserId,
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version),
                    answers,
                    protectedVariables,
                    interviewerIdentity.SupervisorId,
                    interviewerIdentity.UserId,
                    interviewKey,
                    assignment.Id
                );

                await this.commandService.ExecuteAsync(createInterviewCommand);
                assignment.CreatedInterviewsCount = (assignment.CreatedInterviewsCount ?? 0) + 1;
                assignmentsRepository.Store(assignment);
                var formatGuid = interviewId.FormatGuid();
                this.lastCreatedInterviewStorage.Store(formatGuid);
                logger.Warn($"Created interview {interviewId} from assigment {assignment.Id}({assignment.Title}) at {DateTime.Now}");
                auditLogService.Write(new CreateInterviewAuditLogEntity(interviewId, assignment.Id, assignment.Title, interviewKey.ToString()));

                return interviewId;
            }
            catch (InterviewException e)
            {
                logger.Error($"Failed to create interview {interviewId}. {e.ToString()}", e);
                await userInteractionService.AlertAsync(string.Format(InterviewerUIResources.FailedToCreateInterview, e.Message), UIResources.Error);
            }

            return null;
        }

        private CancellationTokenSource loadingCancellationTokenSource;

        public async Task<IStatefulInterview> LoadInterviewAndNavigateThereAsync(Guid interviewId)
        {
            this.ProgressDescription = InterviewerUIResources.Interview_Loading;
            this.OperationDescription = InterviewerUIResources.Interview_Loading_Description;
            IsIndeterminate = false;

            this.loadingCancellationTokenSource = new CancellationTokenSource();
            var interviewIdString = interviewId.FormatGuid();

            var progress = new Progress<EventReadingProgress>();
            progress.ProgressChanged += Progress_ProgressChanged;

            try
            {
                this.loadingCancellationTokenSource.Token.ThrowIfCancellationRequested();

                IStatefulInterview interview = await this.interviewRepository.GetAsync(interviewIdString, progress, this.loadingCancellationTokenSource.Token)
                                                                             .ConfigureAwait(false);

                this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

                return interview;
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception exception)
            {
                await this.userInteractionService.AlertAsync(exception.Message, InterviewerUIResources.FailedToLoadInterview);
                this.logger.Error($"Failed to load interview {interviewId}. {exception.ToString()}", exception);
            }
            finally
            {
                progress.ProgressChanged -= Progress_ProgressChanged;
            }

            return null;
        }

        private List<InterviewAnswer> GetAnswers(List<AssignmentDocument.AssignmentAnswer> identifyingAnswers)
        {
            var elements = identifyingAnswers
                .Select(ia => new InterviewAnswer
                {
                    Identity = ia.Identity,
                    Answer = this.answerSerializer.Deserialize<AbstractAnswer>(ia.SerializedAnswer)
                })
                .Where(x => x.Answer != null)
                .ToList();

            return elements;
        }

        private void Progress_ProgressChanged(object sender, EventReadingProgress e)
        {
            var percent = e.Current.PercentOf(e.Maximum);
            this.ProgressDescription = string.Format(InterviewerUIResources.Interview_Loading_With_Percents, percent);
            this.Progress = percent;
        }
    }
}
