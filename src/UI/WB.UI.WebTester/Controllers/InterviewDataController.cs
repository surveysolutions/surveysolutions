using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.WebTester.Controllers
{
    public class InterviewDataController : Enumerator.Native.WebInterview.Controllers.InterviewDataController
    {
        public InterviewDataController(IQuestionnaireStorage questionnaireRepository, IStatefulInterviewRepository statefulInterviewRepository, 
            IWebInterviewNotificationService webInterviewNotificationService, IWebInterviewInterviewEntityFactory interviewEntityFactory) 
            : base(questionnaireRepository, statefulInterviewRepository, webInterviewNotificationService, interviewEntityFactory)
        {
        }
    }
}
