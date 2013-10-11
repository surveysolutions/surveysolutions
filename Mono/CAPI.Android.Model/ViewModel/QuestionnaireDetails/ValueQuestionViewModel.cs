using System;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    
    public class ValueQuestionViewModel : QuestionViewModel
    {

        public ValueQuestionViewModel(InterviewItemId publicKey, string text, QuestionType questionType, object answer, bool enabled, string instructions, string comments, bool valid,  bool mandatory, 
            string validationMessage)
            : base(publicKey, text, questionType, enabled, instructions, comments, valid, mandatory, answer,validationMessage)
        {
        }
        #region Overrides of QuestionViewModel

        public override IQuestionnaireItemViewModel Clone(int[] propagationVector)
        {
            return new ValueQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector),
                                                   this.Text, this.QuestionType, this.AnswerObject,
                                                   this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                                                   this.Comments, this.Status.HasFlag(QuestionStatus.Valid), 
                                                   this.Mandatory,this.ValidationMessage);
        }

        #endregion
    }
}