using Cirrious.MvvmCross.Plugins.Messenger;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class UpdateInterviewEntityStateMessage : MvxMessage
    {
        public UpdateInterviewEntityStateMessage(object sender, int elementPosition)
            : base(sender)
        {
            this.ElementPosition = elementPosition;
        }

        public int ElementPosition { get; private set; }
    }
}