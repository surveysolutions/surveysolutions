using MvvmCross.Plugins.Messenger;

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
