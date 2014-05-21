using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Android.Events
{
    public class AnswerSavedEventArgs : EventArgs
    {
        public AnswerSavedEventArgs(InterviewItemId questionId)
        {
            this.QuestionId = questionId;
        }
        public InterviewItemId QuestionId { get; private set; }
    }
}