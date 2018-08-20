using MvvmCross.Plugin.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Messages
{
    public class StartingLongOperationMessage : MvxMessage
    {
        public StartingLongOperationMessage(object sender) : base(sender)
        {
        }
    }

    public class StopingLongOperationMessage : MvxMessage
    {
        public StopingLongOperationMessage(object sender) : base(sender)
        {
        }
    }
}
