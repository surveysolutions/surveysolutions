using MvvmCross.Plugin.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class SynchronizationCompletedMsg : MvxMessage
    {
        public SynchronizationCompletedMsg(object sender) : base(sender)
        {
        }
    }
}
