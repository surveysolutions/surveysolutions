using System;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.Plugins.Location;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GpsCoordinatesQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        public QuestionHeaderViewModel Header { get; set; }
        public EnablementViewModel Enablement { get; private set; }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set { isInProgress = value; RaisePropertyChanged(); }
        }

        private MvxCoordinates answer;
        public MvxCoordinates Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(); }
        }

        private bool hasAnswer;
        public bool HasAnswer
        {
            get { return hasAnswer; }
            set { hasAnswer = value; RaisePropertyChanged(); }
        }

        private readonly ICommandService commandService;
        private readonly IUserIdentity userIdentity;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IMvxLocationWatcher geoLocationWatcher;
        private readonly IUserInteraction userInteraction;

        private Identity questionIdentity;
        private Guid interviewId;

        public GpsCoordinatesQuestionViewModel(ICommandService commandService, 
            IUserIdentity userIdentity,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            IMvxLocationWatcher geoLocationWatcher,
            IUserInteraction userInteraction,
            QuestionHeaderViewModel questionHeaderViewModel,
            EnablementViewModel enablementViewModel)
        {
            this.commandService = commandService;
            this.userIdentity = userIdentity;
            this.interviewRepository = interviewRepository;
            this.geoLocationWatcher = geoLocationWatcher;
            this.userInteraction = userInteraction;

            this.Header = questionHeaderViewModel;
            this.Enablement = enablementViewModel;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if(interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            
            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.Header.Init(interviewId, entityIdentity);
            this.Enablement.Init(interviewId, entityIdentity);

            var answerModel = interview.GetGpsCoordinatesAnswer(entityIdentity);
            if (answerModel != null)
            {
                this.Answer = new MvxCoordinates()
                {
                    Altitude = answerModel.Altitude,
                    Longitude = answerModel.Longitude,
                    Latitude = answerModel.Latitude,
                    Accuracy = answerModel.Accuracy
                };
                this.HasAnswer = true;
            }
        }

        private IMvxCommand saveAnswerCommand;
        public IMvxCommand SaveAnswerCommand
        {
            get { return saveAnswerCommand ?? (saveAnswerCommand = new MvxCommand(SaveAnswer)); }
        }

        private void SaveAnswer()
        {
            this.IsInProgress = true;
            this.geoLocationWatcher.Start(options: new MvxLocationOptions(),
                success: this.SetGeoLocationAnswer,
                error: async (error) => { await this.TryGetGeoLocationAgain(); });
        }

        private async Task TryGetGeoLocationAgain()
        {
            this.geoLocationWatcher.Stop();
            this.IsInProgress = false;

            if (await this.userInteraction.ConfirmAsync(
                message: UIResources.Interview_GeoLocation_Confirm_NoLocation,
                title: UIResources.ConfirmationText, 
                okButton: UIResources.ConfirmationTryAgainText,
                cancelButton: UIResources.ConfirmationCancelText))
            {
                this.SaveAnswer();
            }
        }

        private void SetGeoLocationAnswer(MvxGeoLocation location)
        {
            this.geoLocationWatcher.Stop();
            this.IsInProgress = false;

            this.commandService.Execute(new AnswerGeoLocationQuestionCommand(
                interviewId: interviewId,
                userId: userIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                accuracy: location.Coordinates.Accuracy ?? 0,
                altitude: location.Coordinates.Altitude ?? 0,
                latitude: location.Coordinates.Latitude,
                longitude: location.Coordinates.Longitude,
                timestamp: location.Timestamp));

            this.HasAnswer = true;
            this.Answer = location.Coordinates;
        }
    }
}