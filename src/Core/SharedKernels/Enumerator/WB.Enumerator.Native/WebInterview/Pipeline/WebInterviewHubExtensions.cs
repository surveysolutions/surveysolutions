using System;
using Microsoft.AspNetCore.SignalR;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public static class WebInterviewHubExtensions
    {
        public static Guid GetInterviewId(this Hub hub)
        {
            var interviewId = hub.GetInterviewIdString();
            return Guid.Parse(interviewId);
        }

        public static string GetInterviewIdString(this Hub hub)
        {
            var http = hub.Context.GetHttpContext();
            return http.Request.Query["interviewId"];
        }
    }
}
