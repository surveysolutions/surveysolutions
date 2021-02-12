using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class SendInvitationsTask : BaseTask
    {
        public SendInvitationsTask(ISchedulerFactory schedulerFactory) 
            : base(schedulerFactory, "Send invitations", typeof(SendInvitationsJob)) { }
    }
}
