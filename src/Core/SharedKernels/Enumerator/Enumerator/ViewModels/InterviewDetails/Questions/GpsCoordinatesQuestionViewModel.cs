using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Location;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class GpsCoordinatesQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel, IDisposable
    {
        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; this.RaisePropertyChanged(); }
        }

        private MvxCoordinates answer;
        public MvxCoordinates Answer
        {
            get { return this.answer; }
            set { this.answer = value; this.RaisePropertyChanged(); }
        }

        private IMvxCommand saveAnswerCommand;
        public IMvxCommand SaveAnswerCommand
        {
            get { return this.saveAnswerCommand ?? (this.saveAnswerCommand = new MvxCommand(async () => await this.SaveAnswerAsync(), () => !this.IsInProgress)); }
        }

        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IEnumeratorSettings settings;
        private readonly IGpsLocationService locationService;

        private Identity questionIdentity;
        private Guid interviewId;

        public QuestionStateViewModel<GeoLocationQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public GpsCoordinatesQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            IEnumeratorSettings settings,
            IGpsLocationService locationService,
            QuestionStateViewModel<GeoLocationQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository;
            this.settings = settings;
            this.locationService = locationService;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        public Identity Identity { get { return this.questionIdentity; } }

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

        private async Task SaveAnswerAsync()
        {
            this.IsInProgress = true;

            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(this.settings.GpsReceiveTimeoutSec));
                var mvxGeoLocation = await this.locationService.GetLocation(cancellationTokenSource.Token);
                await this.SetGeoLocationAnswerAsync(mvxGeoLocation);
            }
            catch (OperationCanceledException)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.GpsQuestion_Timeout);
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        private async Task SetGeoLocationAnswerAsync(MvxGeoLocation location)
        {
            if (location == null)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.GpsQuestion_Timeout);
                return;
            }


            var command = new AnswerGeoLocationQuestionCommand(
                interviewId: this.interviewId,
                userId: this.userId,
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
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
                this.Answer = location.Coordinates;
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public void Dispose()
        {
            this.QuestionState.Dispose();
        }
    }
}