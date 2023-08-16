using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.CreateInterview
{
    public class CreateAndLoadInterviewViewModel : LoadingInterviewViewModel, IMvxViewModel<CreateInterviewViewModelArg>
    {
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IInterviewerPrincipal interviewerPrincipal;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;
        private readonly ICommandService commandService;
        private readonly ILastCreatedInterviewStorage lastCreatedInterviewStorage;
        private readonly ILogger logger;
        private readonly IInterviewAnswerSerializer answerSerializer;
        private readonly IUserInteractionService userInteractionService;
        private readonly ICalendarEventStorage calendarEventStorage;
        private readonly IViewModelEventRegistry viewModelEventRegistry;

        public CreateAndLoadInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            IStatefulInterviewRepository interviewRepository, 
            ICommandService commandService, 
            ILogger logger, 
            IPlainStorage<InterviewView> interviewsRepository, 
            IAssignmentDocumentsStorage assignmentsRepository, 
            IInterviewerPrincipal interviewerPrincipal, 
            IInterviewUniqueKeyGenerator keyGenerator, 
            ILastCreatedInterviewStorage lastCreatedInterviewStorage, 
            IAuditLogService auditLogService, 
            IInterviewAnswerSerializer answerSerializer, 
            IUserInteractionService userInteractionService,
            IJsonAllTypesSerializer serializer,
            ICalendarEventStorage calendarEventStorage,
            IViewModelEventRegistry viewModelEventRegistry) 
            : base(interviewerPrincipal, viewModelNavigationService, interviewRepository, commandService, logger,
                userInteractionService, interviewsRepository, serializer, auditLogService, viewModelEventRegistry)
        {
            this.assignmentsRepository = assignmentsRepository;
            this.interviewerPrincipal = interviewerPrincipal;
            this.keyGenerator = keyGenerator;
            this.commandService = commandService;
            this.lastCreatedInterviewStorage = lastCreatedInterviewStorage;
            this.logger = logger;
            this.answerSerializer = answerSerializer;
            this.userInteractionService = userInteractionService;
            this.calendarEventStorage = calendarEventStorage;
            this.viewModelEventRegistry = viewModelEventRegistry;
        }

        protected int AssignmentId { get; set; }

        public void Prepare(CreateInterviewViewModelArg parameter)
        {
            AssignmentId = parameter.AssignmentId;
            InterviewId = parameter.InterviewId;
        }

        public override Task Initialize()
        {
            if (AssignmentId == 0) throw new ArgumentException(nameof(AssignmentId));
            if (InterviewId == Guid.Empty) throw new ArgumentException(nameof(InterviewId));

            var assignmentDocument = assignmentsRepository.GetById(AssignmentId);
            QuestionnaireTitle = assignmentDocument?.Title;

            return Task.CompletedTask;
        }

        public override void ViewAppeared()
        {
            Task.Run(CreateAndNavigateToInterviewAsync);
        }

        private async Task CreateAndNavigateToInterviewAsync()
        {
            this.viewModelEventRegistry.WriteToLogInfoBySubscribers();
            this.viewModelEventRegistry.Reset();
            var interviewId = await CreateInterviewAsync(this.AssignmentId, this.InterviewId);
            if (!interviewId.HasValue)
            {
                await this.ViewModelNavigationService.NavigateToDashboardAsync();
                return;
            }

            await LoadAndNavigateToInterviewAsync(interviewId.Value);
        }

        protected async Task<Guid?> CreateInterviewAsync(int assignmentId, Guid interviewId)
        {
            IsIndeterminate = true;
            this.ProgressDescription = EnumeratorUIResources.Interview_Creating;
            this.OperationDescription = EnumeratorUIResources.Interview_Creating_Description;

            try
            {
                var assignment = this.assignmentsRepository.GetById(assignmentId);

                if (assignment.CreatedInterviewsCount.HasValue && assignment.Quantity.HasValue &&
                    (assignment.CreatedInterviewsCount.Value >= assignment.Quantity.Value))
                {
                    return null;
                }

                var interviewerIdentity = (IInterviewerUserIdentity)this.interviewerPrincipal.CurrentUserIdentity;

                this.assignmentsRepository.FetchPreloadedData(assignment);
                var questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);

                List<InterviewAnswer> answers = this.GetAnswers(assignment.Answers);
                List<string> protectedVariables = assignment.ProtectedVariables.Select(x => x.Variable).ToList();

                var interviewKey = keyGenerator.Get();
                ICommand createInterviewCommand = new SharedKernels.DataCollection.Commands.Interview.CreateInterview(interviewId,
                    interviewerIdentity.UserId,
                    questionnaireIdentity,
                    answers,
                    protectedVariables,
                    interviewerIdentity.SupervisorId,
                    interviewerIdentity.UserId,
                    interviewKey,
                    assignment.Id,
                    assignment.IsAudioRecordingEnabled, 
                    InterviewMode.CAPI
                );

                this.commandService.Execute(createInterviewCommand);
                assignment.CreatedInterviewsCount = (assignment.CreatedInterviewsCount ?? 0) + 1;
                assignmentsRepository.Store(assignment);

                var calendarEvent = calendarEventStorage.GetCalendarEventForAssigment(assignment.Id);
                if (calendarEvent != null)
                {
                    var createCalendarEvent = new CreateCalendarEventCommand(Guid.NewGuid(), 
                        interviewerIdentity.UserId,
                        calendarEvent.Start,
                        calendarEvent.StartTimezone,
                        interviewId,
                        interviewKey.ToString(),
                        assignment.Id,
                        calendarEvent.Comment,
                        questionnaireIdentity);
                    
                    commandService.Execute(createCalendarEvent);
                }
                
                var formatGuid = interviewId.FormatGuid();
                this.lastCreatedInterviewStorage.Store(formatGuid);
                logger.Warn($"Created interview {interviewId} from assignment {assignment.Id}({assignment.Title}) at {DateTime.Now}");
                auditLogService.Write(new CreateInterviewAuditLogEntity(interviewId, assignment.Id, assignment.Title, interviewKey.ToString()));

                return interviewId;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to create interview {interviewId}. {e}", e);
                await userInteractionService.AlertAsync(string.Format(EnumeratorUIResources.FailedToCreateInterview, e.Message), UIResources.Error);
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
    }
}
