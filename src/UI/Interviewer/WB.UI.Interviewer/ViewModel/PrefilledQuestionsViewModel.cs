using System;
using MvvmCross.Commands;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
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
            IUserInteractionService userInteractionService)
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
        }

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(this.InterviewId));

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () =>
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
            this.Dispose();
        });

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);
        public IMvxCommand SignOutCommand => new MvxAsyncCommand(async () =>
        {
            await this.viewModelNavigationService.SignOutAndNavigateToLoginAsync();
            this.Dispose();
        });

        public IMvxCommand NavigateToMapsCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<MapsViewModel>);


        public async Task NavigateBack()
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
        }

        public override void ViewAppeared()
        {
            var interviewId = Guid.Parse(this.InterviewId);
            
            if (IsAudioRecordingEnabled == true && !isAuditStarting)
            {
                isAuditStarting = true;
                Task.Run(async () =>
                {
                    try
                    {
                        await audioAuditService.StartAudioRecordingAsync(interviewId);
                    }
                    catch (MissingPermissionsException e)
                    {
                        this.userInteractionService.ShowToast(e.Message);
                        await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
                    }
                    isAuditStarting = false;
                });
            }

            base.ViewAppeared();
        }

        private bool isAuditStarting = false;

        public override void ViewDisappearing()
        {
            var interviewId = Guid.Parse(InterviewId);
            
            if (IsAudioRecordingEnabled == true)
                Task.Run(() => audioAuditService.StopAudioRecordingAsync(interviewId));

            base.ViewDisappearing();
        }
    }
}
