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
using Main.Core.Entities.SubEntities;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public class ValueQuestionView : QuestionView
    {
        public ValueQuestionView(Guid publicKey, string text, QuestionType type, string answer, bool enabled, string instructions, string comments, bool valid)
            : base(publicKey, text, type, enabled,instructions,comments,valid)
        {
            Answer = answer;
            Answered = !string.IsNullOrEmpty(answer);
        }

        public string Answer { get; private set; }
    }
}