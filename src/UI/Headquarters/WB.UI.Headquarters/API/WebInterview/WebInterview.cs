using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Ncqrs.Eventing.Sourcing;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
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

        public void StartInterview() 
            => ((StatefulInterview) this.CurrentInterview).EventApplied += this.OnEventApplied;

        public override Task OnDisconnected(bool stopCalled)
        {
            // statefull interview can be removed from cache here
            ((StatefulInterview) this.CurrentInterview).EventApplied -= this.OnEventApplied;

            return base.OnDisconnected(stopCalled);
        }

        private void OnEventApplied(object sender, EventAppliedEventArgs e) => this.Clients.Caller.apply(e.Event.Payload);
    }
}