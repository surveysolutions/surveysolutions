using System;
using System.Collections.Generic;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class SingleInterviewViewModel : BaseViewModel
    {
        protected readonly ICommandService commandService;
        private readonly VibrationViewModel vibrationViewModel;

        protected SingleInterviewViewModel(
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            VibrationViewModel vibrationViewModel)
            : base(principal, viewModelNavigationService)
        {
            this.commandService = commandService;
            this.vibrationViewModel = vibrationViewModel;
        }

        public bool IsSuccessfullyLoaded { get; protected set; }

        public abstract IReadOnlyCollection<string> AvailableLanguages { get; }
        public abstract string CurrentLanguage { get; }
        public abstract IMvxCommand ReloadCommand { get; }

        protected string interviewId;

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
            if (parameters.Data.ContainsKey("interviewId"))
            {
                this.Initialize(parameters.Data["interviewId"]);
            }
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            bundle.Data["interviewId"] = this.interviewId;
        }

        protected void Initialize(string interviewId)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.interviewId = interviewId;
            this.vibrationViewModel.Initialize(interviewId);
        }

        public IMvxCommand SwitchTranslationCommand => new MvxCommand<string>(this.SwitchTranslation);

        private void SwitchTranslation(string language) => this.commandService.Execute(
            new SwitchTranslation(Guid.Parse(this.interviewId), language, this.principal.CurrentUserIdentity.UserId));

        public virtual void Dispose()
        {
            this.vibrationViewModel.Dispose();
        }
    }
}