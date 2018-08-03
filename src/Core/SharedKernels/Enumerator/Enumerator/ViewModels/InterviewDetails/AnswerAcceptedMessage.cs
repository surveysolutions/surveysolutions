using System;
using MvvmCross.Plugin.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class AnswerAcceptedMessage : MvxMessage
    {
        public AnswerAcceptedMessage(object sender, TimeSpan elapsed) : base(sender)
        {
            Elapsed = elapsed;
        }

        public TimeSpan Elapsed { get; }
    }
}
