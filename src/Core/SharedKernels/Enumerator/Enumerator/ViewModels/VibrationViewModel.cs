using System;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class VibrationViewModel : 
        IViewModelEventHandler<AnswersDeclaredInvalid>,
        IViewModelEventHandler<StaticTextsDeclaredInvalid>,
        IDisposable
    {
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IEnumeratorSettings settings;
        private readonly IVirbationService virbationService;

        public VibrationViewModel(
            IViewModelEventRegistry eventRegistry,
            IEnumeratorSettings settings,
            IVirbationService virbationService)
        {
            this.eventRegistry = eventRegistry;
            this.settings = settings;
            this.virbationService = virbationService;
        }

        public void Initialize(string interviewId)
        {
            if (!this.eventRegistry.IsSubscribed(this))
                this.eventRegistry.Subscribe(this, interviewId);
        }

        public void Handle(AnswersDeclaredInvalid @event) => this.Vibrate();
        public void Handle(StaticTextsDeclaredInvalid @event) => this.Vibrate();

        private void Vibrate()
        {
            if (this.settings.VibrateOnError)
                this.virbationService.Vibrate();

        }
        public void Disable() => this.virbationService.Disable();
        public void Enable() => this.virbationService.Enable();

        public void Dispose() => this.eventRegistry.Unsubscribe(this);
    }
}
