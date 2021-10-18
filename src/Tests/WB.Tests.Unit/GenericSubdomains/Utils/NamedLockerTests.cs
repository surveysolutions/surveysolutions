using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.GenericSubdomains.Utils
{
    [TestOf(typeof(NamedLocker))]
    public class NamedLockerTests
    {
        private class CounterHolder  { public long Value { get; set; } }

        [Test]
        public async Task should_not_allow_more_then_one_parallel_execution_for_same_name()
        {
            var locker = new NamedLocker();
            var counter1 = new CounterHolder();
            var counter2 = new CounterHolder();
            var iterations = 10000;

            var tasks = new []
            {
                Task.Run(() => Runner("key1", counter1)),
                Task.Run(() => Runner("key1", counter1)),
                Task.Run(() => Runner("key1", counter1)),
                Task.Run(() => Runner("key1", counter1)),

                Task.Run(() => Runner("key2", counter2)),
                Task.Run(() => Runner("key2", counter2)),
                Task.Run(() => Runner("key2", counter2)),
                Task.Run(() => Runner("key2", counter2)),
            };

            await Task.WhenAll(tasks);

            Assert.Pass();

            void Runner(string key, CounterHolder counter)
            {
                while (counter.Value < iterations)
                {
                    locker.RunWithLock(key, () =>
                    {
                        var result = counter.Value++;
                        Thread.SpinWait(iterations / 2);

                        Assert.That(result, Is.EqualTo(counter.Value - 1), key);
                    });
                }
            }
        }
    }
}
