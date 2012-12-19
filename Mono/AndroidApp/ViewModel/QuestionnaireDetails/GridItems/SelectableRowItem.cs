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
    public class SelectableRowItem : AbstractRowItem
    {
        public SelectableRowItem(Guid publicKey, Guid propagationKey, string text, QuestionType questionType,
                                 bool enabled, bool valid, string comments, string answer)
            : base(publicKey, propagationKey, text, questionType, enabled, valid, comments)
        {
            Answer = answer;
            Answered = answers.Any(a => a.Selected);
        }

    }
}