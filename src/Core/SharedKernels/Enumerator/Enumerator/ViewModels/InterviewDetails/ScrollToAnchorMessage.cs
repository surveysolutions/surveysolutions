using MvvmCross.Plugins.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class ScrollToAnchorMessage: MvxMessage
    {
        public ScrollToAnchorMessage(object sender, int anchorElementIndex)
            : base(sender)
        {
            this.AnchorElementIndex = anchorElementIndex;
        }

        public int AnchorElementIndex { get; private set; }
    }
}