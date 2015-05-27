using System;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.Plugins.Location;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GpsCoordinatesQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
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

        private IMvxCommand saveAnswerCommand;
        public IMvxCommand SaveAnswerCommand
        {
            get { return saveAnswerCommand ?? (saveAnswerCommand = new MvxCommand(SaveAnswer, () => !this.IsInProgress)); }
        }

        private readonly IUserIdentity userIdentity;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IMvxLocationWatcher geoLocationWatcher;
        private readonly IUserInteraction userInteraction;

        private Identity questionIdentity;
        private Guid interviewId;

        public QuestionStateViewModel<GeoLocationQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public GpsCoordinatesQuestionViewModel(
            IUserIdentity userIdentity,
            IStatefullInterviewRepository interviewRepository,
            IMvxLocationWatcher geoLocationWatcher,
            IUserInteraction userInteraction,
            QuestionStateViewModel<GeoLocationQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.userIdentity = userIdentity;
            this.interviewRepository = interviewRepository;
            this.geoLocationWatcher = geoLocationWatcher;
            this.userInteraction = userInteraction;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if(interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            
            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var answerModel = interview.GetGpsCoordinatesAnswer(entityIdentity);
            if (answerModel != null && answerModel.IsAnswered)
            {
                this.Answer = new MvxCoordinates()
                {
                    Altitude = answerModel.Altitude,
                    Longitude = answerModel.Longitude,
                    Latitude = answerModel.Latitude,
                    Accuracy = answerModel.Accuracy
                };
            }
        }

        private void SaveAnswer()
        {
            this.IsInProgress = true;

            if (this.geoLocationWatcher.Started) return;

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

        private async void SetGeoLocationAnswer(MvxGeoLocation location)
        {
            this.geoLocationWatcher.Stop();

            var command = new AnswerGeoLocationQuestionCommand(
                interviewId: interviewId,
                userId: userIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                accuracy: location.Coordinates.Accuracy ?? 0,
                altitude: location.Coordinates.Altitude ?? 0,
                latitude: location.Coordinates.Latitude,
                longitude: location.Coordinates.Longitude,
                timestamp: location.Timestamp);

            try
            {
                await this.Answering.SendAnswerQuestionCommand(command);
                QuestionState.ExecutedAnswerCommandWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                QuestionState.ProcessAnswerCommandException(ex);
            }

            this.Answer = location.Coordinates;
            this.IsInProgress = false;
        }
    }
}