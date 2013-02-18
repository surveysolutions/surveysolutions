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
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;
using Main.Core.Entities.SubEntities;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public class ValueQuestionViewModel : QuestionViewModel
    {
        public ValueQuestionViewModel(ItemPublicKey publicKey, string text, QuestionType type, string answer, bool enabled, string instructions, string comments, bool valid, bool mandatory)
            : base(publicKey, text, type, enabled, instructions, comments, valid, mandatory, answer)
        {
           // Answer = answer;
         
        }

      /*  public ValueQuestionViewModel(AbstractQuestionRowItem rosterItem, HeaderItem headerItem) : base(rosterItem, headerItem)
        {
            Answer = rosterItem.Text;
        }*/

      //  public string Answer { get; private set; }
    }
}