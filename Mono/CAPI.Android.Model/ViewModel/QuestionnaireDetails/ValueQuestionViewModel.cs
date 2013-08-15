using System;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    
    public class ValueQuestionViewModel : QuestionViewModel
    {
        
        public ValueQuestionViewModel(ItemPublicKey publicKey, string text, QuestionType questionType, object answer, bool enabled, string instructions, string comments, bool valid, bool capital, bool mandatory, 
            string validationMessage)
            : base(publicKey, text, questionType, enabled, instructions, comments, valid, mandatory, capital, answer,validationMessage)
        {
        }
        #region Overrides of QuestionViewModel

        public override IQuestionnaireItemViewModel Clone(int[] propagationVector)
        {
            return new ValueQuestionViewModel(new ItemPublicKey(this.PublicKey.PublicKey, propagationVector),
                                                   this.Text, this.QuestionType, this.AnswerObject,
                                                   this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                                                   this.Comments, this.Status.HasFlag(QuestionStatus.Valid), this.Capital,
                                                   this.Mandatory,this.ValidationMessage);
        }

        #endregion
    }
}