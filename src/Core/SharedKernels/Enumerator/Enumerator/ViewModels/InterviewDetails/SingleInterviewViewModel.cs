using System;
using System.Collections.Generic;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class SingleInterviewViewModel : BaseViewModel
    {
        private readonly ICommandService commandService;

        protected SingleInterviewViewModel(
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService)
            : base(principal, viewModelNavigationService)
        {
            this.commandService = commandService;
        }

        public bool IsSuccessfullyLoaded { get; protected set; }

        public abstract IReadOnlyCollection<string> AvailableLanguages { get; }
        public abstract string CurrentLanguage { get; }
        public abstract IMvxCommand ReloadCommand { get; }

        protected string interviewId;

        protected void Initialize(string interviewId)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.interviewId = interviewId;
        }

        public IMvxCommand SwitchTranslationCommand => new MvxCommand<string>(this.SwitchTranslation);

        private void SwitchTranslation(string language) => this.commandService.Execute(
            new SwitchTranslation(Guid.Parse(this.interviewId), language, this.principal.CurrentUserIdentity.UserId));
    }
}