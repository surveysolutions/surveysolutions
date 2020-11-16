using Microsoft.Extensions.Configuration;

namespace WB.Enumerator.Native.WebInterview
{
    public static class WebInterviewConfigExtensions
    {
        public static bool IsLazyInterviewNotificationEnabled(this IConfiguration configuration)
        {
            return configuration.GetValue("WebInterview::LazyNotification", false);
        }
    }
}
