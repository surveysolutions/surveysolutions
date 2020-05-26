using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public interface IPipelineModule
    {
        Task OnConnected(Hub hub);
        Task OnDisconnected(Hub hub, Exception exception);

        protected string GetInterviewId(Hub hub)
        {
            var http = hub.Context.GetHttpContext();
            return http.Request.Query["interviewId"];
        }
    }
}
