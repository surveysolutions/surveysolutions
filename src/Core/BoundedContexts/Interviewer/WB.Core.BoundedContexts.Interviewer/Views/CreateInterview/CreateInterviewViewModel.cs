using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
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
    public class CreateInterviewViewModel : ProgressViewModel<CreateInterviewViewModelArg>
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

        public CreateInterviewViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IAssignmentDocumentsStorage assignmentsRepository,
            IInterviewerPrincipal interviewerPrincipal,
            IInterviewUniqueKeyGenerator keyGenerator,
            ICommandService commandService,
            ILastCreatedInterviewStorage lastCreatedInterviewStorage,
            ILogger logger,
            IAuditLogService auditLogService,
            IInterviewAnswerSerializer answerSerializer,
            IUserInteractionService userInteractionService) 
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
            Task.Run(CreateInterviewAndNavigateThereAsync);
        }

        private async Task CreateInterviewAndNavigateThereAsync()
        {
            try
            {
                var assignment = this.assignmentsRepository.GetById(AssignmentId);

                if (assignment.CreatedInterviewsCount.HasValue && assignment.Quantity.HasValue &&
                    (assignment.CreatedInterviewsCount.Value >= assignment.Quantity.Value))
                {
                    await this.viewModelNavigationService.NavigateToDashboardAsync();
                    return;
                }

                var interviewerIdentity = this.interviewerPrincipal.CurrentUserIdentity;

                this.assignmentsRepository.FetchPreloadedData(assignment);
                var questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);

                List<InterviewAnswer> answers = this.GetAnswers(assignment.Answers);
                List<string> protectedVariables = assignment.ProtectedVariables.Select(x => x.Variable).ToList();

                var interviewKey = keyGenerator.Get();
                ICommand createInterviewCommand = new SharedKernels.DataCollection.Commands.Interview.CreateInterview(InterviewId,
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
                var formatGuid = InterviewId.FormatGuid();
                this.lastCreatedInterviewStorage.Store(formatGuid);
                logger.Warn($"Created interview {InterviewId} from assigment {assignment.Id}({assignment.Title}) at {DateTime.Now}");
                auditLogService.Write(new CreateInterviewAuditLogEntity(InterviewId, assignment.Id, assignment.Title, interviewKey.ToString()));
                await this.viewModelNavigationService.NavigateToAsync<LoadingViewModel, LoadingViewModelArg>(new LoadingViewModelArg { InterviewId = InterviewId });
            }
            catch (InterviewException e)
            {
                logger.Error(e.Message, e);
                await userInteractionService.AlertAsync(string.Format(InterviewerUIResources.FailedToCreateInterview, e.Message), UIResources.Error);
            }
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
    }
}
