using System;
using MvvmCross.Commands;
using System.Threading.Tasks;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Interviewer.ViewModel
{
    public class PrefilledQuestionsViewModel : BasePrefilledQuestionsViewModel
    {
        private readonly IAudioAuditService audioAuditService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IMvxMainThreadAsyncDispatcher asyncDispatcher;

        public PrefilledQuestionsViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelNavigationService viewModelNavigationService,
            IEnumeratorSettings enumeratorSettings,
            ILogger logger,
            IPrincipal principal,
            ICommandService commandService,
            ICompositeCollectionInflationService compositeCollectionInflationService,
            VibrationViewModel vibrationViewModel,
            IAudioAuditService audioAuditService,
            IUserInteractionService userInteractionService,
            IMvxMainThreadAsyncDispatcher asyncDispatcher)
            : base(
                interviewViewModelFactory,
                questionnaireRepository,
                interviewRepository,
                viewModelNavigationService,
                enumeratorSettings,
                logger,
                principal,
                commandService,
                compositeCollectionInflationService,
                vibrationViewModel)
        {
            this.audioAuditService = audioAuditService;
            this.userInteractionService = userInteractionService;
            this.asyncDispatcher = asyncDispatcher;
        }

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () => await this.ViewModelNavigationService.NavigateToPrefilledQuestionsAsync(this.InterviewId));

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () =>
        {
            await this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
            this.Dispose();
        });

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.ViewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);
        public IMvxCommand SignOutCommand => new MvxAsyncCommand(async () =>
        {
            await this.ViewModelNavigationService.SignOutAndNavigateToLoginAsync();
            this.Dispose();
        });

        public IMvxCommand NavigateToMapsCommand => new MvxAsyncCommand(this.ViewModelNavigationService.NavigateToAsync<MapsViewModel>);


        public async Task NavigateBack()
        {
            await this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
        }

        public override void ViewAppeared()
        {
            if (!this.Principal.IsAuthenticated)
            {
                this.ViewModelNavigationService.NavigateToLoginAsync().WaitAndUnwrapException();
                return;
            }

            var interviewId = Guid.Parse(this.InterviewId);
            
            if (IsAudioRecordingEnabled == true && !isAuditStarting)
            {
                isAuditStarting = true;
                asyncDispatcher.ExecuteOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await audioAuditService.StartAudioRecordingAsync(interviewId).ConfigureAwait(false);
                        isAuditStarting = false;
                    }
                    catch (MissingPermissionsException e)
                    {
                        this.userInteractionService.ShowToast(e.Message);
                        await this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
                    }
                });
            }

            base.ViewAppeared();
        }

        private bool isAuditStarting = false;

        public override void ViewDisappearing()
        {
            var interviewId = Guid.Parse(InterviewId);
            
            if (IsAudioRecordingEnabled == true)
                audioAuditService.StopAudioRecording(interviewId);

            base.ViewDisappearing();
        }
    }
}
