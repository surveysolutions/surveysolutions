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
        public ValueQuestionViewModel(ItemPublicKey publicKey, string text, QuestionType type, string answer, bool enabled, string instructions, string comments, bool valid, bool capital, bool mandatory, string validationExpression,
            string validationMessage)
            : base(publicKey, text, type, enabled, instructions, comments, valid, mandatory, capital, answer, validationExpression, validationMessage)
        {
           // Answer = answer;
         
        }

      /*  public ValueQuestionViewModel(AbstractQuestionRowItem rosterItem, HeaderItem headerItem) : base(rosterItem, headerItem)
        {
            Answer = rosterItem.Text;
        }*/

      //  public string Answer { get; private set; }

        #region Overrides of QuestionViewModel

        public override IQuestionnaireItemViewModel Clone(Guid propagationKey)
        {
            return new ValueQuestionViewModel(new ItemPublicKey(this.PublicKey.PublicKey, propagationKey),
                                                   this.Text, this.QuestionType, this.AnswerString,
                                                   this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                                                   this.Comments, this.Status.HasFlag(QuestionStatus.Valid), this.Capital,
                                                   this.Mandatory,this.ValidationExpression,this.ValidationMessage);
        }

        public override string AnswerObject
        {
            get { return AnswerString; }
        }

        #endregion
    }
}