using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class SingleInterviewViewModel : BaseViewModel<InterviewViewModelArgs>, IDisposable
    {
        public IEnumeratorSettings EnumeratorSettings { get; }
        protected readonly ICommandService commandService;
        protected readonly VibrationViewModel vibrationViewModel;

        protected SingleInterviewViewModel(
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IEnumeratorSettings enumeratorSettings,
            VibrationViewModel vibrationViewModel)
            : base(principal, viewModelNavigationService)
        {
            EnumeratorSettings = enumeratorSettings;
            this.commandService = commandService;
            this.vibrationViewModel = vibrationViewModel;
        }

        public override void Prepare(InterviewViewModelArgs parameter)
        {
            this.InterviewId = parameter.InterviewId;
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            if (this.InterviewId == null)
            {
                await this.ViewModelNavigationService.NavigateToDashboardAsync().ConfigureAwait(false);
                return;
            }

            this.vibrationViewModel.Initialize(this.InterviewId);
        }

        public bool IsSuccessfullyLoaded { get; protected set; }

        public abstract IReadOnlyCollection<string> AvailableLanguages { get; }
        
        public abstract string DefaultLanguageName { get; }
        public abstract string CurrentLanguage { get; }
        public abstract IMvxCommand ReloadCommand { get; }

        protected string InterviewId;

        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);
            if (state.Data.ContainsKey("interviewId"))
            {
                this.InterviewId = state.Data["interviewId"];
            }
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            bundle.Data["interviewId"] = this.InterviewId;
        }

        public IMvxCommand SwitchTranslationCommand => new MvxCommand<string>(this.SwitchTranslation);

        private void SwitchTranslation(string language) => this.commandService.Execute(
            new SwitchTranslation(Guid.Parse(this.InterviewId), language, this.Principal.CurrentUserIdentity.UserId));

        public virtual void Dispose()
        {
            this.vibrationViewModel.Dispose();
        }
    }
}
