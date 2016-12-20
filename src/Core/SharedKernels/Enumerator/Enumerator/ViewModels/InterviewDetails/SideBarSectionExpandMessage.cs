using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class SideBarSectionExpandMessage : MvxMessage
    {
        public readonly Identity ExpandedGroup;

        public SideBarSectionExpandMessage(object sender, Identity expandedGroup)
            : base(sender)
        {
            this.ExpandedGroup = expandedGroup;
        }
    }
}