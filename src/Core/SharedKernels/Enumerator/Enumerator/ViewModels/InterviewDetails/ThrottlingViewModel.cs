using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class ThrottlingViewModel : IDisposable
    {
        private readonly Timer timer = null;
        private bool hasPendingAction = false;
        private bool isDisposed = false;

        protected internal int ThrottlePeriod { get; set; } = Constants.ThrottlePeriod;
        private readonly IUserInterfaceStateService userInterfaceStateService;
        private Func<Task> callbackAction;

        public ThrottlingViewModel(IUserInterfaceStateService userInterfaceStateService)
        {
            this.userInterfaceStateService = userInterfaceStateService;
            this.timer = new Timer(async _ => { await TimerCallback(); }, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Init(Func<Task> action)
        {
            this.callbackAction = action;
        }

        public async Task ExecuteActionIfNeeded()
        {
            if (isDisposed) return;

            if (this.ThrottlePeriod == 0)
            {
                await InvokeCallbackIfSet();
                return;
            }
            
            if (!hasPendingAction)
            {
                this.userInterfaceStateService.ThrottledActionStarted();
                this.hasPendingAction = true;
            }
            this.ResetTimer();
        }

        public void CancelPendingAction()
        {
            FinishThrottledAction();
            CancelTimer();
        }

        public void Dispose()
        {
            FinishThrottledAction();

            isDisposed = true;
            callbackAction = null;
            timer?.Dispose();
        }

        private async Task TimerCallback()
        {
            try
            {
                await InvokeCallbackIfSet();
            }
            finally
            {
                FinishThrottledAction();
            }
        }

        private async Task InvokeCallbackIfSet()
        {
            if (this.callbackAction != null)
            {
                await callbackAction();
            }
        }

        private void CancelTimer()
        {
            if (isDisposed) return;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void ResetTimer()
        {
            if (isDisposed) return;
            timer.Change(ThrottlePeriod, Timeout.Infinite);
        }

        private void FinishThrottledAction()
        {
            if (!hasPendingAction)
                return;
            userInterfaceStateService.ThrottledActionFinished();
            this.hasPendingAction = false;
        }
    }
}
