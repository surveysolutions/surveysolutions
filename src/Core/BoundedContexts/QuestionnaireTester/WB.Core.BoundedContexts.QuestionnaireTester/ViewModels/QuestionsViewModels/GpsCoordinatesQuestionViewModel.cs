using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Location;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
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
            get { return saveAnswerCommand ?? (saveAnswerCommand = new MvxCommand(async () => await SaveAnswer(), () => !this.IsInProgress)); }
        }

        private readonly IUserIdentity userIdentity;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ISettingsProvider settingsProvider;
        private readonly IGpsLocationService locationService;

        private Identity questionIdentity;
        private Guid interviewId;

        public QuestionStateViewModel<GeoLocationQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public GpsCoordinatesQuestionViewModel(
            IUserIdentity userIdentity,
            IStatefulInterviewRepository interviewRepository,
            ISettingsProvider settingsProvider,
            IGpsLocationService locationService,
            QuestionStateViewModel<GeoLocationQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.userIdentity = userIdentity;
            this.interviewRepository = interviewRepository;
            this.settingsProvider = settingsProvider;
            this.locationService = locationService;

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
            if (answerModel.IsAnswered)
            {
                this.Answer = new MvxCoordinates
                {
                    Longitude = answerModel.Longitude.Value,
                    Latitude = answerModel.Latitude.Value,
                    Altitude = answerModel.Altitude,
                    Accuracy = answerModel.Accuracy
                };
            }
        }

        private async Task SaveAnswer()
        {
            this.IsInProgress = true;

            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(this.settingsProvider.GpsReceiveTimeoutSec));
                var mvxGeoLocation = await this.locationService.GetLocation(cancellationTokenSource.Token);
                await this.SetGeoLocationAnswer(mvxGeoLocation);
            }
            catch (OperationCanceledException)
            {
                QuestionState.Validity.MarkAnswerAsInvalidWithMessage(UIResources.GpsQuestion_Timeout);
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        private async Task SetGeoLocationAnswer(MvxGeoLocation location)
        {
            if (location == null)
            {
                QuestionState.Validity.MarkAnswerAsInvalidWithMessage(UIResources.GpsQuestion_Timeout);
                return;
            }


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
                this.QuestionState.Validity.ExecutedWithoutExceptions();
                this.Answer = location.Coordinates;
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }
    }
}