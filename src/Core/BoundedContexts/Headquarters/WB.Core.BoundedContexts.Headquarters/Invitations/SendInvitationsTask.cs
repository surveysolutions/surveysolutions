using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class SendInvitationsTask : BaseTask
    {
        public SendInvitationsTask(IScheduler scheduler) 
            : base(scheduler, "Send invitations", typeof(SendInvitationsJob)) { }
    }
}
