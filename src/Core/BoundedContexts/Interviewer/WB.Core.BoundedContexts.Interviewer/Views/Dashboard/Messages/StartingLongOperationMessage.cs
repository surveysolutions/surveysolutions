﻿using MvvmCross.Plugins.Messenger;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages
{
    public class StartingLongOperationMessage : MvxMessage
    {
        public StartingLongOperationMessage(object sender) : base(sender)
        {
        }
    }
}