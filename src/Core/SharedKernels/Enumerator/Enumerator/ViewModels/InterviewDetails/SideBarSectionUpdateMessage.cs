using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class SideBarSectionUpdateMessage : MvxMessage
    {
        public readonly Identity UpdatedGroup;

        public SideBarSectionUpdateMessage(object sender, Identity updatedGroup)
            : base(sender)
        {
            this.UpdatedGroup = updatedGroup;
        }
    }
}