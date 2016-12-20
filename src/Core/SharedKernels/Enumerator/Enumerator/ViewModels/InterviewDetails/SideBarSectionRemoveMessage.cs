using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class SideBarSectionRemoveMessage : MvxMessage
    {
        public readonly Identity RemovedGroup;

        public SideBarSectionRemoveMessage(object sender, Identity removedGroup)
            : base(sender)
        {
            this.RemovedGroup = removedGroup;
        }
    }
}