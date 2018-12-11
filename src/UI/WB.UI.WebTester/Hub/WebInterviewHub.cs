using System;
using System.Diagnostics;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Hub
{
    [HubName(@"interview")]
    public class WebInterviewHub : WebInterview
    {
        private IEvictionNotifier evictionNotify => this.serviceLocator.GetInstance<IEvictionNotifier>();

        public override void CompleteInterview(CompleteInterviewRequest completeInterviewRequest)
        {
            var interviewId = Guid.Parse(this.CallerInterviewId);
            evictionNotify.Evict(interviewId);
        }
    }
}
