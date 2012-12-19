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

namespace AndroidApp.ViewModel.QuestionnaireDetails.GridItems
{
    public class ValueRowItem:AbstractRowItem
    {
        public ValueRowItem(Guid publicKey, Guid propagationKey, string text, QuestionType questionType, bool enabled, bool valid, string comments, string answer) : base(publicKey, propagationKey, text, questionType, enabled, valid, comments)
        {
            this.Answer = answer;
            this.Answered = !string.IsNullOrEmpty(answer);
        }

    }
}