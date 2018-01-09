using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;

namespace WB.UI.WebTester.Hub
{
    [HubName(@"interview")]
    public class WebInterviewHub : WebInterview
    {
        public WebInterviewHub(IStatefulInterviewRepository statefulInterviewRepository, ICommandService commandService, IQuestionnaireStorage questionnaireRepository, IWebInterviewNotificationService webInterviewNotificationService, IWebInterviewInterviewEntityFactory interviewEntityFactory) : base(statefulInterviewRepository, commandService, questionnaireRepository, webInterviewNotificationService, interviewEntityFactory)
        {
        }

        public override void CompleteInterview(CompleteInterviewRequest completeInterviewRequest)
        {
            Clients.All.shutDown();
        }
    }
}