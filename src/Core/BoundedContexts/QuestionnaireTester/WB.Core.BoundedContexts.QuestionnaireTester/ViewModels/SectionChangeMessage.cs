using Cirrious.MvvmCross.Plugins.Messenger;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SectionChangeMessage : MvxMessage
    {
        public SectionChangeMessage(object sender)
            : base(sender)
        {
        }
    }
}
