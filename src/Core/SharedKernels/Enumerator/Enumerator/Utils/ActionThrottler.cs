using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class ActionThrottler
    {
        private bool isThrottled = false;

        public async Task RunDelayed(Action action, TimeSpan delay, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.isThrottled)
            {
                return;
            }

            this.isThrottled = true;
            
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

            if (this.isThrottled != true || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            action();

            this.isThrottled = false;
        }

        public void Complete()
        {
            this.isThrottled = false;
        }
    }
}
