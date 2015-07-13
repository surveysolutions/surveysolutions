using Cirrious.MvvmCross.Plugins.Messenger;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
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

    public class SectionChangeMessage : MvxMessage
    {
        public SectionChangeMessage(object sender)
            : base(sender)
        {
        }
    }
}
