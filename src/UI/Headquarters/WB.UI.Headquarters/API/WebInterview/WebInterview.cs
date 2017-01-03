using Microsoft.AspNet.SignalR;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview : Hub
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        
        private string interviewId => Context.ConnectionId;
        private IStatefulInterview CurrentInterview => this.statefulInterviewRepository.Get(this.interviewId);

        public WebInterview(IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
        }
    }
}