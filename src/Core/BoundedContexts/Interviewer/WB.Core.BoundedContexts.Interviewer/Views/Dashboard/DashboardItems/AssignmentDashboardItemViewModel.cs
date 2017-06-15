using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class AssignmentDashboardItemViewModel : MvxNotifyPropertyChanged, IDashboardItem
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IIdentifyingAnswerConverter identifyingAnswerConverter;
        private readonly IInterviewAnswerSerializer answerSerializer;
        private readonly ICommandService commandService;
        private readonly IInterviewerPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IExternalAppLauncher externalAppLauncher;

        public AssignmentDashboardItemViewModel(
            ICommandService commandService,
            IInterviewerPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMvxMessenger messenger,
            IPlainStorage<InterviewView> interviewViewRepository,
            IExternalAppLauncher externalAppLauncher,
            IQuestionnaireStorage questionnaireRepository,
            IIdentifyingAnswerConverter identifyingAnswerConverter, 
            IInterviewAnswerSerializer answerSerializer)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.identifyingAnswerConverter = identifyingAnswerConverter;
            this.answerSerializer = answerSerializer;
            this.commandService = commandService;
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.messenger = messenger;
            this.interviewViewRepository = interviewViewRepository;
            this.externalAppLauncher = externalAppLauncher;
        }

        private QuestionnaireIdentity questionnaireIdentity;

        public void Init(AssignmentDocument assignmentDocument)
        {
            this.assignment = assignmentDocument;
            this.questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);

            var identifyingData = assignment.Answers.Where(id => id.Identity.Id != assignment.LocationQuestionId && id.IsIdentifying).ToList();
            this.PrefilledQuestions = GetPrefilledQuestions(identifyingData.Take(3));
            this.DetailedPrefilledQuestions = GetPrefilledQuestions(identifyingData.Skip(3));
            this.GpsLocation = this.GetAssignmentLocation(assignment);

            this.Title = string.Format(InterviewerUIResources.Dashboard_Assignment_CardTitle, this.assignment.Id.ToString()) + ": ";

            var interviewsByAssignmentCount = this.interviewViewRepository.Count(interview => interview.Assignment == this.assignment.Id);
            if (this.assignment.Quantity.HasValue)
            {
                var interviewsLeftByAssignmentCount = Math.Max(0, this.assignment.Quantity.Value - this.assignment.InterviewsCount - interviewsByAssignmentCount);
                this.Title += InterviewerUIResources.Dashboard_AssignmentCard_TitleCountdown.FormatString(interviewsLeftByAssignmentCount.ToString());
            }
            else
            {
                this.Title += InterviewerUIResources.Dashboard_AssignmentCard_TitleCountdown_Unlimited;
            }

            this.Comment = string.Format(InterviewerUIResources.DashboardItem_AssignmentCreatedComment, interviewsByAssignmentCount);
            this.ReceivedDate = assignment.ReceivedDateUtc.ToLocalTime().ToString("MMM d");
            this.ReceivedTime = assignment.ReceivedDateUtc.ToLocalTime().ToString("HH:mm");
        }

        public string ReceivedTime { get; set; }

        public string ReceivedDate { get; private set; }

        private AssignmentDocument assignment;

        public string QuestionnaireName => string.Format(InterviewerUIResources.DashboardItem_Title, this.assignment.Title, this.questionnaireIdentity.Version);
        public string Comment { get; private set; }

        public List<PrefilledQuestion> PrefilledQuestions { get; private set; }
        public List<PrefilledQuestion> DetailedPrefilledQuestions { get; private set; }

        public string Title { get; private set; }

        public InterviewGpsCoordinatesView GpsLocation { get; private set; }
        public bool HasGpsLocation => this.GpsLocation != null;

        public bool AllowToCreateNewInterview
        {
            get
            {
                if (this.assignment.Quantity.HasValue)
                {
                    var interviewsByAssignmentCount = this.interviewViewRepository.Count(interview => interview.Assignment == this.assignment.Id);
                    var interviewsLeftByAssignmentCount = this.assignment.Quantity.Value - this.assignment.InterviewsCount - interviewsByAssignmentCount;
                    return interviewsLeftByAssignmentCount > 0;
                }
                return true;
            }
        }

        public IMvxAsyncCommand CreateNewInterviewCommand => new MvxAsyncCommand(CreateNewInterviewAsync, () => AllowToCreateNewInterview);

        private async Task CreateNewInterviewAsync()
        {
            RaiseStartingLongOperation();
            var interviewId = Guid.NewGuid();
            var interviewerIdentity = this.principal.CurrentUserIdentity;

            List<InterviewAnswer> answers = GetAnswersToIdentifyingQuestions(this.assignment.Answers);

            ICommand createInterviewCommand = new CreateInterviewWithPreloadedData(interviewId,
                    interviewerIdentity.UserId,
                    this.questionnaireIdentity.QuestionnaireId,
                    this.questionnaireIdentity.Version,
                    answers,
                    DateTime.UtcNow,
                    interviewerIdentity.SupervisorId,
                    interviewerIdentity.UserId,
                    null,
                    this.assignment.Id
                );
            
            await this.commandService.ExecuteAsync(createInterviewCommand);
            this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
        }

        private List<InterviewAnswer> GetAnswersToIdentifyingQuestions(List<AssignmentDocument.AssignmentAnswer> identifyingAnswers)
        {
            var elements = identifyingAnswers
                .Select(ia => new InterviewAnswer
                {
                    Identity = ia.Identity,
                    Answer = this.ConvertToAbstractAnswer(ia)
                })
                .Where(x => x.Answer != null)
                .ToList();

            return elements;
        }

        private AbstractAnswer ConvertToAbstractAnswer(AssignmentDocument.AssignmentAnswer assignmentAnswer)
        {
            return this.answerSerializer.Deserialize<AbstractAnswer>(assignmentAnswer.SerializedAnswer);
        }

        private void RaiseStartingLongOperation()
        {
            messenger.Publish(new StartingLongOperationMessage(this));
        }

        private List<PrefilledQuestion> GetPrefilledQuestions(IEnumerable<AssignmentDocument.AssignmentAnswer> identifyingAnswers)
        {
            return identifyingAnswers.Select(fi => new PrefilledQuestion
                {
                    Answer = fi.AnswerAsString,
                    Question = fi.Question
                }).ToList();
        }

        private InterviewGpsCoordinatesView GetAssignmentLocation(AssignmentDocument assignmentDocument)
        {
            if (assignmentDocument.LocationQuestionId.HasValue && assignmentDocument.LocationLatitude.HasValue && assignmentDocument.LocationLongitude.HasValue)
            {
                return new InterviewGpsCoordinatesView
                {
                    Latitude = assignmentDocument.LocationLatitude ?? 0,
                    Longitude = assignmentDocument.LocationLongitude ?? 0
                };
            }

            return null;
        }

        public IMvxCommand NavigateToGpsLocationCommand
        {
            get { return new MvxCommand(this.NavigateToGpsLocation, () => this.HasGpsLocation); }
        }

        public bool HasExpandedView { get => this.PrefilledQuestions.Count > 0; }

        private void NavigateToGpsLocation()
        {
            this.externalAppLauncher.LaunchMapsWithTargetLocation(this.GpsLocation.Latitude, this.GpsLocation.Longitude);
        }
    }
}