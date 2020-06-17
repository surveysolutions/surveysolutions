using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils
{
    public class ActionThrottlerTests
    {
        [Test]
        public void should_run_action_within_delay()
        {
            var delay = new ActionThrottler();
            var isPass = false;

            delay.RunDelayed(() => isPass = true, TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);

            Thread.Sleep(1000);

            Assert.True(isPass);
        }

        [Test]
        public void should_not_run_action_if_canceled()
        {
            var delay = new ActionThrottler();
            var isPass = false;

            delay.RunDelayed(() => isPass = true, TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);

            delay.Complete();

            Thread.Sleep(500);

            Assert.False(isPass);
        }

        [Test]
        public void should_run_in_background_without_await()
        {
            var delay = new ActionThrottler();
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            delay.RunDelayed(() =>
            {
                if (tcs.Task.IsCompleted)
                {
                    return;
                }
                Assert.Fail();
            }, TimeSpan.FromMilliseconds(200)).ConfigureAwait(false);
            
            tcs.SetResult(true);
            
            Assert.Pass();
        }

        [Test]
        public void should_not_run_action_if_canceled_by_token()
        {
            var delay = new ActionThrottler();
            var isPass = false;
            var cts = new CancellationTokenSource();

            delay.RunDelayed(() => isPass = true, TimeSpan.FromMilliseconds(100), cts.Token).ConfigureAwait(false);
            cts.Cancel();

            Thread.Sleep(500);

            Assert.False(isPass);
        }
    }
}
