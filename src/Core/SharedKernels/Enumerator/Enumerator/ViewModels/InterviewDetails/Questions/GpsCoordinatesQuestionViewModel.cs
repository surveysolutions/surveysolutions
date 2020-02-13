﻿using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Plugin.Geolocator.Abstractions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class GpsCoordinatesQuestionViewModel :
        MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IViewModelEventHandler<AnswersRemoved>,
        ICompositeQuestion,
        IDisposable
    {
        private GpsLocation answer;
        public GpsLocation Answer
        {
            get => this.answer;
            set
            {
                this.SetProperty(ref this.answer, value);
                this.LocationInfo = value == null ? ((string, string)?) null : ($"{value.Latitude}, {value.Longitude}", $"https://www.google.com/maps?q={value.Latitude},{value.Longitude}");
            }
        }

        private (string text, string url)? locationInfo;
        public (string text, string url)? LocationInfo {
            get => this.locationInfo;
            set => this.SetProperty(ref this.locationInfo, value);
        }

        public bool ShowLocationOnMap => this.settings.ShowLocationOnMap &&
                                         this.googleApiService.GetPlayServicesConnectionStatus() == GoogleApiConnectionStatus.Success;

        public IMvxAsyncCommand SaveAnswerCommand => new MvxAsyncCommand(this.SaveAnswerAsync, () => !this.Answering.InProgress);
        public IMvxCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);

        private readonly Guid userId;
        private readonly ILogger logger;
        private readonly IGoogleApiService googleApiService;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IEnumeratorSettings settings;
        private readonly IViewModelEventRegistry liteEventRegistry;
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
            IViewModelEventRegistry liteEventRegistry,
            ILogger logger,
            IGoogleApiService googleApiService)
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
            this.googleApiService = googleApiService;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(interviewId);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);

            var questionModel = interview.GetGpsQuestion(entityIdentity);
            if (questionModel.IsAnswered())
            {
                var gpsAnswer = questionModel.GetAnswer().Value;

                this.Answer = new GpsLocation(gpsAnswer.Accuracy, gpsAnswer.Altitude, gpsAnswer.Latitude,
                    gpsAnswer.Longitude, DateTimeOffset.MinValue);
            }

            this.liteEventRegistry.Subscribe(this, interviewId);
        }

        private async Task RemoveAnswerAsync()
        {
            try
            {
                var command = new RemoveAnswerCommand(this.interviewId, this.userId, this.questionIdentity);
                await this.Answering.SendRemoveAnswerCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private async Task SaveAnswerAsync()
        {
            this.Answering.StartInProgressIndicator();
            this.userInterfaceStateService.NotifyRefreshStarted();
            try
            {
                var mvxGeoLocation = await this.locationService.GetLocation(this.settings.GpsReceiveTimeoutSec,
                    this.settings.GpsDesiredAccuracy).ConfigureAwait(false);

                this.userInterfaceStateService.NotifyRefreshFinished();
                await this.SetGeoLocationAnswerAsync(mvxGeoLocation);
            }
            catch (GeolocationException e)
            {
                switch (e.Error)
                {
                    case GeolocationError.PositionUnavailable:
                        await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.GpsQuestion_Timeout);
                        break;
                    case GeolocationError.Unauthorized:
                        await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.GpsQuestion_MissingPermissions);
                        break;
                }
            }
            catch (MissingPermissionsException)
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.GpsQuestion_MissingPermissions);
            }
            catch (OperationCanceledException)
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.GpsQuestion_Timeout);
            }
            catch (Exception e)
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.GpsQuestion_Timeout);
                logger.Warn(e.Message, e);
            }
            finally
            {
                this.userInterfaceStateService.NotifyRefreshFinished();
                this.Answering.FinishInProgressIndicator();
            }
        }

        private async Task SetGeoLocationAnswerAsync(GpsLocation location)
        {
            if (location == null)
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.GpsQuestion_Timeout);
                return;
            }

            var command = new AnswerGeoLocationQuestionCommand(
                interviewId: this.interviewId,
                userId: this.userId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
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
            this.liteEventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    this.Answer = null;
                }
            }
        }
    }
}
