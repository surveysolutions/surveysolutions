using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Events
{
    public class AnswerSetEventArgs : EventArgs
    {
        public AnswerSetEventArgs(InterviewItemId questionId, string answerSting)
        {
            QuestionId = questionId;
            AnswerSting = answerSting;
        }

        public InterviewItemId QuestionId { get; private set; }
        public string AnswerSting { get; private set; }
    }
}