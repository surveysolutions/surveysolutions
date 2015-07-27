using Cirrious.MvvmCross.Plugins.Messenger;

namespace WB.Core.BoundedContexts.Tester.ViewModels
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