using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class ValueQuestionViewModel : QuestionViewModel
    {
        public ValueQuestionViewModel(InterviewItemId publicKey, ValueVector<Guid> questionRosterScope, string text,
            QuestionType questionType, object answer, bool enabled,
            string instructions, string comments, bool valid, bool mandatory,
            string validationMessage, string variable, IEnumerable<string> substitutionReference, bool? isInteger, int? countOfDecimalPlaces)
            : base(
                publicKey, questionRosterScope, text, questionType, enabled, instructions, comments, valid, mandatory, answer,
                validationMessage, variable, substitutionReference)

        {
            this.IsInteger = isInteger;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;
        }

        public bool? IsInteger = null;
        public string Mask = null;
        public int? CountOfDecimalPlaces = null;

        #region Overrides of QuestionViewModel

        public override IQuestionnaireItemViewModel Clone(decimal[] propagationVector)
        {
            return new ValueQuestionViewModel(new InterviewItemId(this.PublicKey.Id, propagationVector), this.QuestionRosterScope,
                this.SourceText, this.QuestionType, this.AnswerObject,
                this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                this.Comments, this.Status.HasFlag(QuestionStatus.Valid),
                this.Mandatory, this.ValidationMessage, this.Variable, this.SubstitutionReferences, this.IsInteger,
                this.CountOfDecimalPlaces);
        }
        public override string AnswerString
        {
            get
            {
                if (this.AnswerObject == null)
                    return "";
                if (this.QuestionType != QuestionType.Numeric)
                    return this.AnswerObject.ToString();
                if (this.AnswerObject is int)
                    return string.Format(CultureInfo.CurrentCulture, "{0:n0}", (int)this.AnswerObject);
                if (this.AnswerObject is decimal)
                    return ((decimal)this.AnswerObject).ToString("##,###.############################", CultureInfo.CurrentCulture);
                return this.AnswerObject.ToString();
            }
        }
        #endregion
    }
}