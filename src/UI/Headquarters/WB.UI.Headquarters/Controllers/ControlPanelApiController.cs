using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Synchronization;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    public class ControlPanelApiController : ApiController
    {
        private readonly InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext;

        public ControlPanelApiController(InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext)
        {
            this.interviewDetailsDataProcessorContext = interviewDetailsDataProcessorContext;
        }

        public InterviewDetailsSchedulerViewModel InterviewDetails()
        {
            return new InterviewDetailsSchedulerViewModel()
            {
                Messages = this.interviewDetailsDataProcessorContext.GetMessages().ToArray()
            };
        }
    }
}