using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class WebInterviewNotificationService : IWebInterviewNotificationService
    {
        // Singleton instance
        private static readonly Lazy<IHubContext> _instance = new Lazy<IHubContext>(
            () => GlobalHost.ConnectionManager.GetHubContext<WebInterview>());

        public void RefreshEntities(Guid interviewId, Identity[] questions)
        {
            var questionToRefresh = questions.Select(q => q.ToString()).ToArray();
            _instance.Value.Clients.Group(interviewId.FormatGuid()).refreshEntities(questionToRefresh);
        }
    }
}