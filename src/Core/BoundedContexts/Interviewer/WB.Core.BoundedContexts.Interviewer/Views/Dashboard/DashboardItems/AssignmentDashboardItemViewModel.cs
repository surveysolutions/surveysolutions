using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class AssignmentDashboardItemViewModel : MvxNotifyPropertyChanged, IDashboardItem
    {
        private readonly IInterviewAnswerSerializer answerSerializer;
        private readonly ICommandService commandService;
        private readonly IInterviewerPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IExternalAppLauncher externalAppLauncher;
        private readonly IAssignmentDocumentsStorage assignmentDocumentsStorage;

        public AssignmentDashboardItemViewModel(
            ICommandService commandService,
            IInterviewerPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMvxMessenger messenger,
            IPlainStorage<InterviewView> interviewViewRepository,
            IExternalAppLauncher externalAppLauncher,
            IAssignmentDocumentsStorage assignmentDocumentsStorage,
            IInterviewAnswerSerializer answerSerializer)
        {
            this.answerSerializer = answerSerializer;
            this.commandService = commandService;
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.messenger = messenger;
            this.interviewViewRepository = interviewViewRepository;
            this.externalAppLauncher = externalAppLauncher;
            this.assignmentDocumentsStorage = assignmentDocumentsStorage;
        }

        private QuestionnaireIdentity questionnaireIdentity;

        public void Init(AssignmentDocument assignmentDocument, DashboardViewModel dashboardViewModel)
        {
            this.assignment = assignmentDocument;
            this.questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);

            var identifyingData = assignment.IdentifyingAnswers;

            this.PrefilledQuestions = GetPrefilledQuestions(identifyingData.Take(3));
            this.DetailedPrefilledQuestions = GetPrefilledQuestions(identifyingData.Skip(3));
            this.GpsLocation = this.GetAssignmentLocation(assignment);

            dashboardViewModel.InterviewsCountChanged += (sender, args) => this.Refresh();

            this.Refresh();
            this.ReceivedDate = assignment.ReceivedDateUtc.ToLocalTime().ToString("MMM d");
            this.ReceivedTime = assignment.ReceivedDateUtc.ToLocalTime().ToString("HH:mm");
        }

        private void Refresh()
        {
            var newTitle = string.Format(InterviewerUIResources.Dashboard_Assignment_CardTitle, this.assignment.Id) + ": ";

            var interviewsByAssignmentCount =
                this.interviewViewRepository.Count(interview => interview.Assignment == this.assignment.Id);
            if (this.assignment.Quantity.HasValue)
            {
                var interviewsLeftByAssignmentCount = Math.Max(0, this.assignment.Quantity.Value - interviewsByAssignmentCount);
                newTitle += InterviewerUIResources.Dashboard_AssignmentCard_TitleCountdown.FormatString(interviewsLeftByAssignmentCount);
            }
            else
            {
                newTitle += InterviewerUIResources.Dashboard_AssignmentCard_TitleCountdown_Unlimited;
            }

            this.RaisePropertyChanged(() => AllowToCreateNewInterview);
            this.Title = newTitle;
            this.Comment = string.Format(InterviewerUIResources.DashboardItem_AssignmentCreatedComment, interviewsByAssignmentCount);
        }

        public string ReceivedTime { get; set; }

        public string ReceivedDate { get; private set; }

        private AssignmentDocument assignment;
        private string title;
        private string comment;

        public string QuestionnaireName => string.Format(InterviewerUIResources.DashboardItem_Title, this.assignment.Title, this.questionnaireIdentity.Version);

        public string Comment
        {
            get => this.comment;
            private set => this.RaiseAndSetIfChanged(ref this.comment, value);
        }

        public List<PrefilledQuestion> PrefilledQuestions { get; private set; }
        public List<PrefilledQuestion> DetailedPrefilledQuestions { get; private set; }

        public string Title
        {
            get => this.title;
            private set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        public InterviewGpsCoordinatesView GpsLocation { get; private set; }
        public bool HasGpsLocation => this.GpsLocation != null;

        public bool AllowToCreateNewInterview
        {
            get
            {
                if (this.assignment.Quantity.HasValue)
                {
                    var interviewsByAssignmentCount = this.interviewViewRepository.Count(interview => interview.Assignment == this.assignment.Id);
                    var interviewsLeftByAssignmentCount = this.assignment.Quantity.Value - interviewsByAssignmentCount;
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
            this.assignmentDocumentsStorage.FetchPreloadedData(this.assignment);

            List<InterviewAnswer> answers = this.GetAnswers(this.assignment.Answers);

            ICommand createInterviewCommand = new CreateInterview(interviewId,
                    interviewerIdentity.UserId,
                    this.questionnaireIdentity.QuestionnaireId, 
                    this.questionnaireIdentity.Version,
                    answers,
                    DateTime.UtcNow, 
                    interviewerIdentity.SupervisorId,
                    null,
                    null,
                    this.assignment.Id);

            await this.commandService.ExecuteAsync(createInterviewCommand);
            this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
        }

        private List<InterviewAnswer> GetAnswers(List<AssignmentDocument.AssignmentAnswer> identifyingAnswers)
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

        public bool HasExpandedView => this.PrefilledQuestions.Count > 0;

        private void NavigateToGpsLocation()
        {
            this.externalAppLauncher.LaunchMapsWithTargetLocation(this.GpsLocation.Latitude, this.GpsLocation.Longitude);
        }
    }
}