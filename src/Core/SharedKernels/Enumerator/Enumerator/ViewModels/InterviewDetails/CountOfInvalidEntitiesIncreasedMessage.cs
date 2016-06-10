using MvvmCross.Plugins.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CountOfInvalidEntitiesIncreasedMessage : MvxMessage
    {
        public CountOfInvalidEntitiesIncreasedMessage(object sender) : base(sender)
        {
        }
    }
}