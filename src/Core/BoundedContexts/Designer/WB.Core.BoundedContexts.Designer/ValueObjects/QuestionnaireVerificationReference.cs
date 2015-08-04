using System;
using System.Diagnostics;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
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

        protected bool Equals(QuestionnaireVerificationReference other)
        {
            return this.Id.Equals(other.Id) && this.Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QuestionnaireVerificationReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Id.GetHashCode() * 397) ^ (int)this.Type;
            }
        }
    }
}