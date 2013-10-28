using System;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public class QuestionnaireVerificationException : QuestionnaireException
    {
        internal QuestionnaireVerificationException(string message, Exception innerException, QuestionnaireVerificationError[] errors)
            : base(message, innerException)
        {
            this.Errors = errors;
        }

        internal QuestionnaireVerificationException(string message, QuestionnaireVerificationError[] errors)
            : base(message)
        {
            this.Errors = errors;
        }

        public QuestionnaireVerificationError[] Errors { get; private set; }
    }
}