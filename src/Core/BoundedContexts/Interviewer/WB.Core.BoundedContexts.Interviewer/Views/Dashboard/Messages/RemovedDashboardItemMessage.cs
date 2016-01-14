using MvvmCross.Plugins.Messenger;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages
{
    public class RemovedDashboardItemMessage : MvxMessage
    {
        public RemovedDashboardItemMessage(object sender) : base(sender)
        {
        }
    }
}