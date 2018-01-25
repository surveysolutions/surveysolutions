using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable;
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
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;

        public WebInterviewHub(IStatefulInterviewRepository statefulInterviewRepository, ICommandService commandService, IQuestionnaireStorage questionnaireRepository, IWebInterviewNotificationService webInterviewNotificationService, IWebInterviewInterviewEntityFactory interviewEntityFactory,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager) : 
            base(statefulInterviewRepository, commandService, questionnaireRepository, webInterviewNotificationService, interviewEntityFactory)
        {
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
        }

        public override void CompleteInterview(CompleteInterviewRequest completeInterviewRequest)
        {
            appdomainsPerInterviewManager.TearDown(GetCallerInterview().Id);
            this.Clients.Group(base.GetCallerInterview().Id.FormatGuid()).shutDown();
        }
    }
}