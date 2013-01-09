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
    public class ValueRowItem:AbstractQuestionRowItem
    {
        public ValueRowItem(Guid publicKey, Guid propagationKey, string answer, QuestionType questionType, bool enabled, bool valid, string comments)
            : base(publicKey, propagationKey, enabled, comments, questionType, valid, answer)
        {
            /*this.Answer = answer;
            this.Answered = !string.IsNullOrEmpty(answer);*/
        }

    }
}