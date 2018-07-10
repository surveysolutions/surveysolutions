using MvvmCross.Plugin.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class DashboardChangedMsg : MvxMessage
    {
        public DashboardChangedMsg(object sender) : base(sender)
        {
        }
    }
}
