using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class SideBarSectionCollapseMessage : MvxMessage
    {
        public readonly Identity CollapsedGroup;

        public SideBarSectionCollapseMessage(object sender, Identity collapsedGroup)
            : base(sender)
        {
            this.CollapsedGroup = collapsedGroup;
        }
    }
}