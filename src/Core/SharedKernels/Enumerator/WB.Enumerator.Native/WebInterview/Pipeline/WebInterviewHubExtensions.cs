using System;
using Microsoft.AspNetCore.SignalR;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public static class WebInterviewHubExtensions
    {
        public static Guid GetInterviewId(this Hub hub)
        {
            var http = hub.Context.GetHttpContext(); 
            var interviewId = http.Request.Query["interviewId"];
            return Guid.Parse(interviewId);
        }
    }
}
