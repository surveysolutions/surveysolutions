using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Logging.Slack;

namespace WB.UI.Shared.Web.Slack
{
    public class SlackMessageThrottler : ISlackMessageThrottler
    {
        readonly ConcurrentDictionary<FatalExceptionType, CacheEntry> timespans
            = new ConcurrentDictionary<FatalExceptionType, CacheEntry>();

        readonly SemaphoreSlim locker = new SemaphoreSlim(1);

        public async Task Throttle(SlackFatalMessage message, TimeSpan throttleAmount, Func<Task> sendAction)
        {
            await locker.WaitAsync();

            try
            {
                var cache = timespans.GetOrAdd(message.Type, ch => new CacheEntry
                {
                    Color = message.Color
                });

                if (cache.Color != message.Color || cache.Timer == null || cache.Timer.Elapsed > throttleAmount)
                {
                    // skipping initial notification on success status
                    if (cache.Color == null && message.Color == SlackColor.Good) return;

                    // always skip recurring success notifications
                    if (cache.Color == SlackColor.Good && message.Color == SlackColor.Good) return;

                    await sendAction();
                    cache.Color = message.Color;

                    if(cache.Timer == null)
                        cache.Timer = Stopwatch.StartNew();
                    else
                        cache.Timer.Restart();
                }
            }
            finally
            {
                locker.Release();
            }
        }

        private class CacheEntry
        {
            public SlackColor? Color { get; set; }
            public Stopwatch Timer { get; set; } // = Stopwatch.StartNew();
        }
    }
}
