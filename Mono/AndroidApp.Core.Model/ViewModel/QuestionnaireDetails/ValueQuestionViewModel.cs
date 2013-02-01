using System;
using Main.Core.Entities.SubEntities;

namespace AndroidApp.Core.Model.ViewModel.QuestionnaireDetails
{
    public class ValueQuestionViewModel : QuestionViewModel
    {
        public ValueQuestionViewModel(ItemPublicKey publicKey, string text, QuestionType type, string answer, bool enabled, string instructions, string comments, bool valid, bool capital, bool mandatory, string validationExpression,
            string validationMessage)
            : base(publicKey, text, type, enabled, instructions, comments, valid, mandatory, capital, answer, validationExpression, validationMessage)
        {
        }

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