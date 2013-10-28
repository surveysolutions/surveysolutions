using System;

namespace WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects.Verification
{
    public class QuestionnaireVerificationReference
    {
        public QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType type, Guid id)
        {
            this.Type = type;
            this.Id = id;
        }

        public QuestionnaireVerificationReferenceType Type { get; private set; }
        public Guid Id { get; private set; }
    }
}