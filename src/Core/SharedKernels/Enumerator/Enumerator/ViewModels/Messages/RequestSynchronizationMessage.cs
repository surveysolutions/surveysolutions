using MvvmCross.Plugin.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class RequestSynchronizationMessage : MvxMessage
    {
        public RequestSynchronizationMessage(object sender) : base(sender)
        {
        }
    }
}
