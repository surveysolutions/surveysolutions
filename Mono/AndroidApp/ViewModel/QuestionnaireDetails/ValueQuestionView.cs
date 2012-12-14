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
        public ValueQuestionView(Guid publicKey, string text, QuestionType type, string answer) : base(publicKey, text, type)
        {
            Answer = answer;
        }

        public string Answer { get; private set; }
    }
}