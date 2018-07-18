using System;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.GenericSubdomains.Utils
{
    [TestOf(typeof(NamedAsyncLocker))]
    public class NamedAsyncLockerTests
    {
        private class CounterHolder  { public long Value { get; set; } }

        [Test()]
        public void ensure_that_without_lock_test_will_fail()
        {
            Assert.ThrowsAsync<Exception>(async () =>
            {
                var counter = new CounterHolder();
                var iterations = 1000;

                var tasks = new[]
                {
                    Task.Run(async () => await NotLockedRunner()),
                    Task.Run(async () => await NotLockedRunner()),
                    Task.Run(async () => await NotLockedRunner()),
                    Task.Run(async () => await NotLockedRunner()),
                    Task.Run(async () => await NotLockedRunner())
                };

                await Task.WhenAll(tasks);

                async Task NotLockedRunner()
                {
                    while (counter.Value < iterations)
                    {
                        var result = counter.Value++;
                        await Task.Delay(TimeSpan.FromMilliseconds(0.1));

                        if (counter.Value - 1 != result)
                        {
                            throw new Exception();
                        }
                    }
                }
            });
        }

        [Test]
        public async Task should_not_allow_more_then_one_parallel_execution_for_same_name()
        {
            var locker = new NamedAsyncLocker();
            var counter1 = new CounterHolder();
            var counter2 = new CounterHolder();
            var iterations = 1000;

            var tasks = new []
            {
                Task.Run(async () => await Runner("key1", counter1)),
                Task.Run(async () => await Runner("key1", counter1)),
                Task.Run(async () => await Runner("key1", counter1)),

                Task.Run(async () => await Runner("key2", counter2)),
                Task.Run(async () => await Runner("key2", counter2)),
                Task.Run(async () => await Runner("key2", counter2)),
                Task.Run(async () => await Runner("key2", counter2)),
            };

            await Task.WhenAll(tasks);

            Assert.Pass();

            async Task Runner(string key, CounterHolder counter)
            {
                while (counter.Value < iterations)
                {
                    await locker.RunWithLockAsync(key, async () =>
                    {
                        var result = counter.Value++;
                        await Task.Delay(TimeSpan.FromMilliseconds(0.1));

                        Assert.That(result, Is.EqualTo(counter.Value - 1), key);
                        return true;
                    });
                }
            }
        }
    }
}