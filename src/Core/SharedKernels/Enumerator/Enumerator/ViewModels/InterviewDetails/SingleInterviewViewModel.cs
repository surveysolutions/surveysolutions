using System;
using System.Collections.Generic;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class SingleInterviewViewModel : BaseViewModel,
        ILiteEventHandler<AnswersDeclaredInvalid>,
        ILiteEventHandler<StaticTextsDeclaredInvalid>
    {
        private readonly ICommandService commandService;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IEnumeratorSettings settings;
        private readonly IVirbationService virbationService;

        protected SingleInterviewViewModel(
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            ILiteEventRegistry eventRegistry,
            IEnumeratorSettings settings,
            IVirbationService virbationService)
            : base(principal, viewModelNavigationService)
        {
            this.commandService = commandService;
            this.eventRegistry = eventRegistry;
            this.settings = settings;
            this.virbationService = virbationService;
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

            if (!this.eventRegistry.IsSubscribed(this))
                this.eventRegistry.Subscribe(this, this.interviewId);
        }

        public IMvxCommand SwitchTranslationCommand => new MvxCommand<string>(this.SwitchTranslation);

        private void SwitchTranslation(string language) => this.commandService.Execute(
            new SwitchTranslation(Guid.Parse(this.interviewId), language, this.principal.CurrentUserIdentity.UserId));

        public void Handle(AnswersDeclaredInvalid @event) => this.Vibrate();

        public void Handle(StaticTextsDeclaredInvalid @event) => this.Vibrate();

        private void Vibrate()
        {
            if (this.settings.VibrateOnError)
                this.virbationService.Vibrate();

        }

        public virtual void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
        }
    }
}