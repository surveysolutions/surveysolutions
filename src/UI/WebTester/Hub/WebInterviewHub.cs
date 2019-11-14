using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Pipeline;

namespace WB.UI.WebTester.Hub
{
    //[HubName(@"interview")]
    public class WebInterviewHub : WebInterview
    {
        public WebInterviewHub(IPipelineModule[] hubPipelineModules) : base(hubPipelineModules)
        {
        }
    }
}
