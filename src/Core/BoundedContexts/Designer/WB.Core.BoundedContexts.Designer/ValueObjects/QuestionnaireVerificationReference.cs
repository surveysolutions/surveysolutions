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
        public int? FailedValidationConditionIndex { get; set; }

        protected bool Equals(QuestionnaireVerificationReference other)
            => this.Id.Equals(other.Id)
            && this.Type == other.Type
            && this.FailedValidationConditionIndex == other.FailedValidationConditionIndex;

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

        public static QuestionnaireVerificationReference CreateForGroup(Guid groupId)
            => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Group, groupId);

        public static QuestionnaireVerificationReference CreateForVariable(Guid variableId)
            => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Variable, variableId);

        public static QuestionnaireVerificationReference CreateForRoster(Guid rosterId)
            => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Roster, rosterId);

        public static QuestionnaireVerificationReference CreateForQuestion(Guid questionId)
            => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question, questionId);

        public static QuestionnaireVerificationReference CreateForMacro(Guid macroId)
            => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Macro, macroId);

        public static QuestionnaireVerificationReference CreateForLookupTable(Guid lookupTableId)
            => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.LookupTable, lookupTableId);

        public static QuestionnaireVerificationReference CreateForAttachment(Guid attachmentId)
            => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Attachment, attachmentId);

        public static QuestionnaireVerificationReference CreateForStaticText(Guid staticTextId)
            => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.StaticText, staticTextId);

        public static QuestionnaireVerificationReference CreateForTranslation(Guid translationId)
            => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Translation, translationId);
    }
}