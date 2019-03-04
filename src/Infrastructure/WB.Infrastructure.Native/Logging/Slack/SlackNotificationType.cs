using NLog;
using NLog.Targets;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Infrastructure.Native.Logging.Slack
{
    /// <summary>
    /// Slack notification target. 
    /// This is not a generic target. This class will parse and look to certain exceptions.
    /// </summary>
    public sealed class SlackFatalNotificationsTarget : Target
    {
        protected override void Write(LogEventInfo logEvent)
        {
            var slack = ServiceLocator.Current.GetInstance<ISlackApiClient>();

            slack.SendMessageAsync(new SlackFatalMessage
            {
                Exception = logEvent.Exception,
                Message = logEvent.FormattedMessage,
                Type = logEvent.Exception.GetFatalType() ?? FatalExceptionType.None,
                Color = SlackColor.Danger
            }).Wait();
        }
    }
}
