using System;
using System.Diagnostics;

namespace WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects
{
    [DebuggerDisplay("{Type} {Id}")]
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