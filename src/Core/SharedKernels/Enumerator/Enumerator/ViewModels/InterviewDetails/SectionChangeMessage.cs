using MvvmCross.Plugin.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class SectionChangeMessage : MvxMessage
    {
        public SectionChangeMessage(object sender)
            : base(sender)
        {
        }
    }
}
