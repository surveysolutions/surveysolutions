using System;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.Entities;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Interviewer.ViewModel
{
    public class PrefilledQuestionsViewModel : BasePrefilledQuestionsViewModel
    {
        private readonly IAuditLogService auditLogService;

        public PrefilledQuestionsViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelNavigationService viewModelNavigationService,
            ILogger logger,
            IPrincipal principal,
            ICommandService commandService,
            ICompositeCollectionInflationService compositeCollectionInflationService,
            VibrationViewModel vibrationViewModel,
            IAuditLogService auditLogService)
            : base(
                interviewViewModelFactory,
                questionnaireRepository,
                interviewRepository,
                viewModelNavigationService,
                logger,
                principal,
                commandService,
                compositeCollectionInflationService,
                vibrationViewModel)
        {
            this.auditLogService = auditLogService;
        }

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(this.InterviewId));

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () =>
        {
            auditLogService.Write(new CloseInterviewAuditLogEntity(InterviewId, null));
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
    }
}
