using MvvmCross.Plugins.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class InterviewCompletedMessage : MvxMessage
    {
        public InterviewCompletedMessage(object sender) : base(sender)
        {
        }
    }
}