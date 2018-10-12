using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class ThrottlingViewModel : IDisposable
    {
        private Timer timer = null;
        private bool hasPendingAction = false;

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

        private async Task TimerCallback()
        {
            if (this.callbackAction != null)
            {
                await callbackAction();
            }

            if (!hasPendingAction) return;
            userInterfaceStateService.ThrottledActionFinished();
            this.hasPendingAction = false;
        }

        public void ResetTimer()
        {
            timer.Change(ThrottlePeriod, Timeout.Infinite);
        }

        public async Task ExecuteActionIfNeeded()
        {
            if (this.ThrottlePeriod == 0)
            {
                await callbackAction();
            }
            else
            {
                if (!hasPendingAction)
                {
                    this.userInterfaceStateService.ThrottledActionStarted();
                    this.hasPendingAction = true;
                }
                this.ResetTimer();
            }
        }

        public void CancelPendingAction()
        {
            if (hasPendingAction)
            {
                this.userInterfaceStateService.ThrottledActionFinished();
                this.hasPendingAction = false;
            }
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
