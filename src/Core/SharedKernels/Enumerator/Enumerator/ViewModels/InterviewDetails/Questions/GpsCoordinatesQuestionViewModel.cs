using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class GpsCoordinatesQuestionViewModel :
        MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswerRemoved>,
        ICompositeQuestion,
        IDisposable
    {
        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; this.RaisePropertyChanged(); }
        }

        private GpsLocation answer;
        public GpsLocation Answer
        {
            get { return this.answer; }
            set { this.answer = value; this.RaisePropertyChanged(); }
        }

        private IMvxCommand saveAnswerCommand;
        public IMvxCommand SaveAnswerCommand
        {
            get { return this.saveAnswerCommand ?? (this.saveAnswerCommand = new MvxCommand(async () => await this.SaveAnswerAsync(), () => !this.IsInProgress)); }
        }

        private IMvxCommand answerRemoveCommand;

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return this.answerRemoveCommand ??
                       (this.answerRemoveCommand = new MvxCommand(async () => await this.RemoveAnswer()));
            }
        }

        private async Task RemoveAnswer()
        {
            try
            {
                var command = new RemoveAnswerCommand(
                    this.interviewId,
                    this.userId,
                    new Identity(this.questionIdentity.Id,
                        this.questionIdentity.RosterVector),
                    DateTime.UtcNow);
                await this.Answering.SendRemoveAnswerCommandAsync(command);

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }

            this.AnswerRemoved?.Invoke(this, EventArgs.Empty);
        }

        private readonly Guid userId;
        public event EventHandler AnswerRemoved;
        private readonly ILogger logger;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IEnumeratorSettings settings;
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IGpsLocationService locationService;
        private readonly IUserInterfaceStateService userInterfaceStateService;

        private Identity questionIdentity;
        private Guid interviewId;

        private readonly QuestionStateViewModel<GeoLocationQuestionAnswered> questionState;
        public IQuestionStateViewModel QuestionState => this.questionState;
        public QuestionInstructionViewModel InstructionViewModel { get; }

        public AnsweringViewModel Answering { get; private set; }

        public GpsCoordinatesQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            IEnumeratorSettings settings,
            IGpsLocationService locationService,
            QuestionStateViewModel<GeoLocationQuestionAnswered> questionStateViewModel,
            IUserInterfaceStateService userInterfaceStateService,
            AnsweringViewModel answering,
             QuestionInstructionViewModel instructionViewModel,
            ILiteEventRegistry liteEventRegistry,
            ILogger logger)
        {
            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository;
            this.settings = settings;
            this.locationService = locationService;
            this.userInterfaceStateService = userInterfaceStateService;

            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.liteEventRegistry = liteEventRegistry;
            this.logger = logger;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(interviewId);

            this.liteEventRegistry.Subscribe(this, interviewId);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);

            var answerModel = interview.GetGpsCoordinatesAnswer(entityIdentity);
            if (answerModel.IsAnswered)
            {
                this.Answer = new GpsLocation(answerModel.Accuracy.GetValueOrDefault(),
                    answerModel.Altitude.GetValueOrDefault(),
                    answerModel.Latitude.GetValueOrDefault(),
                    answerModel.Longitude.GetValueOrDefault(),
                    DateTimeOffset.MinValue);
            }
        }

        private async Task SaveAnswerAsync()
        {
            this.IsInProgress = true;
            this.userInterfaceStateService.NotifyRefreshStarted();
            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(this.settings.GpsReceiveTimeoutSec));
                var mvxGeoLocation = await this.locationService.GetLocation(cancellationTokenSource.Token, this.settings.GpsDesiredAccuracy);

                this.userInterfaceStateService.NotifyRefreshFinished();
                await this.SetGeoLocationAnswerAsync(mvxGeoLocation);
            }
            catch (OperationCanceledException)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.GpsQuestion_Timeout);
            }
            catch (Exception e)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.GpsQuestion_Timeout);
                logger.Warn(e.Message, e);
            }
            finally
            {
                this.userInterfaceStateService.NotifyRefreshFinished();
                this.IsInProgress = false;
            }
        }

        private async Task SetGeoLocationAnswerAsync(GpsLocation location)
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
                accuracy: location.Accuracy,
                altitude: location.Altitude,
                latitude: location.Latitude,
                longitude: location.Longitude,
                timestamp: location.Timestamp);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();

                this.Answer = location;
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public void Dispose()
        {
            this.QuestionState.Dispose();
            this.liteEventRegistry.Unsubscribe(this);
        }

        public void Handle(AnswerRemoved @event)
        {
            if (@event.QuestionId == this.questionIdentity.Id &&
                @event.RosterVector.SequenceEqual(this.questionIdentity.RosterVector))
            {
                this.Answer = null;
            }
        }
    }
}