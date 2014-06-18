using System;
using System.Diagnostics;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects
{
    [DebuggerDisplay("{Type} {Id}")]
    public class QuestionnaireVerificationReference
    {
        public QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType type, Guid id)
        {
            this.Type = type;
            this.Id = id;
            this.ItemId = id.FormatGuid();
        }

        public QuestionnaireVerificationReferenceType Type { get; private set; }
        public Guid Id { get; private set; }
        public string ItemId { get; private set; }
    }
}