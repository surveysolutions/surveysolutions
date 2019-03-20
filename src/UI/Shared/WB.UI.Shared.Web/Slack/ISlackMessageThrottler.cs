using System;
using System.Threading.Tasks;
using WB.Infrastructure.Native.Logging.Slack;

namespace WB.UI.Shared.Web.Slack
{
    public interface ISlackMessageThrottler
    {
        Task Throttle(SlackFatalMessage message, TimeSpan throttleAmount, Func<Task> sendAction);
    }
}
