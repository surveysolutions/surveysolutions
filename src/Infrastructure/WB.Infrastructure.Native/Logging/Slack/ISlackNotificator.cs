using System;
using System.Threading.Tasks;

namespace WB.Infrastructure.Native.Logging.Slack
{
    public interface ISlackApiClient
    {
        Task SendMessageAsync(SlackFatalMessage message);
    }



    public class SlackFatalMessage
    {
        public FatalExceptionType Type { get; set; }
        public string Message { get; set; }
        public SlackColor? Color { get; set; }
        public Exception Exception { get; set; }

    }

    public enum SlackColor { Good, Warning, Danger }
}
