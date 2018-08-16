using MvvmCross.Plugin.Messenger;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages
{
    public class StartingLongOperationMessage : MvxMessage
    {
        public StartingLongOperationMessage(object sender) : base(sender)
        {
        }
    }

    public class StopingLongOperationMessage : MvxMessage
    {
        public StopingLongOperationMessage(object sender) : base(sender)
        {
        }
    }

    public class InterviewRemovedMessage : MvxMessage
    {
        public InterviewRemovedMessage(object sender) : base(sender)
        {
        }
    }
}
