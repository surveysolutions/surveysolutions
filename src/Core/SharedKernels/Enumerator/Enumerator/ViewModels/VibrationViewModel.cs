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
        private readonly IVibrationService vibrationService;

        public VibrationViewModel(
            IViewModelEventRegistry eventRegistry,
            IEnumeratorSettings settings,
            IVibrationService vibrationService)
        {
            this.eventRegistry = eventRegistry;
            this.settings = settings;
            this.vibrationService = vibrationService;
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
                this.vibrationService.Vibrate();

        }
        public void Disable() => this.vibrationService.Disable();
        public void Enable() => this.vibrationService.Enable();

        public void Dispose() => this.eventRegistry.Unsubscribe(this);
    }
}
