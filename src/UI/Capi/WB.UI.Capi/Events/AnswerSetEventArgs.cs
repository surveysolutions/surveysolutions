using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Android.Events
{
    public class AnswerSetEventArgs : EventArgs
    {
        public AnswerSetEventArgs(InterviewItemId questionId, string answerSting)
        {
            this.QuestionId = questionId;
            this.AnswerSting = answerSting;
        }

        public InterviewItemId QuestionId { get; private set; }
        public string AnswerSting { get; private set; }
    }
}