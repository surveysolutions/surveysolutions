using Cirrious.MvvmCross.Plugins.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class InterviewCompleteMessage : MvxMessage
    {
        public InterviewCompleteMessage(object sender) : base(sender)
        {
        }
    }
}