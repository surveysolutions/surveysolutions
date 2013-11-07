using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.CAPI.Views.InterviewDetails
{
    
    public class ValueQuestionViewModel : QuestionViewModel
    {

        public ValueQuestionViewModel(InterviewItemId publicKey, string text, QuestionType questionType, object answer, bool enabled,
            string instructions, string comments, bool valid, bool mandatory,
            string validationMessage, string variable, IEnumerable<string> substitutionReference, bool? isInteger, int? countOfDecimalPlaces)
            : base(publicKey, text, questionType, enabled, instructions, comments, valid, mandatory, answer, validationMessage, variable, substitutionReference)

        {
            this.IsInteger = isInteger;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;
        }

        public bool? IsInteger = null;
        public int? CountOfDecimalPlaces = null;

        #region Overrides of QuestionViewModel

        public override IQuestionnaireItemViewModel Clone(int[] propagationVector)
        {
            return new ValueQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector),
                                                   this.SourceText, this.QuestionType, this.AnswerObject,
                                                   this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                                                   this.Comments, this.Status.HasFlag(QuestionStatus.Valid),
                                                   this.Mandatory, this.ValidationMessage, this.Variable, this.SubstitutionReferences, this.IsInteger, this.CountOfDecimalPlaces);
        }

        #endregion
    }
}