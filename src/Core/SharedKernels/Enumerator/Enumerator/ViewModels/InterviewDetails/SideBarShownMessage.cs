using Cirrious.MvvmCross.Plugins.Messenger;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class SideBarShownMessage : MvxMessage
    {
        public SideBarShownMessage(object sender) : base(sender)
        {
        }
    }
}