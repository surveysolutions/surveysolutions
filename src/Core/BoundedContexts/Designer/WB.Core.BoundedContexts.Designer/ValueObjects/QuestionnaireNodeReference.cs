using System;
using System.Diagnostics;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    [DebuggerDisplay("{Type} {Id}")]
    public class QuestionnaireNodeReference
    {
        public QuestionnaireNodeReference(QuestionnaireVerificationReferenceType type, Guid id)
        {
            this.Type = type;
            this.Id = id;
            this.ItemId = id.FormatGuid();
        }

        public QuestionnaireVerificationReferenceType Type { get; private set; }
        public Guid Id { get; private set; }
        public string ItemId { get; private set; }
        public int? FailedValidationConditionIndex { get; set; }
        public Guid? SectionId { get; set; }

        protected bool Equals(QuestionnaireNodeReference other)
            => this.Id.Equals(other.Id)
            && this.Type == other.Type
            && this.FailedValidationConditionIndex == other.FailedValidationConditionIndex;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QuestionnaireNodeReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Id.GetHashCode() * 397) ^ (int)this.Type;
            }
        }

        public static QuestionnaireNodeReference CreateForGroup(Guid groupId)
            => new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Group, groupId);

        public static QuestionnaireNodeReference CreateForVariable(Guid variableId)
            => new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Variable, variableId);

        public static QuestionnaireNodeReference CreateForRoster(Guid rosterId)
            => new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Roster, rosterId);

        public static QuestionnaireNodeReference CreateForQuestion(Guid questionId)
            => new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Question, questionId);

        public static QuestionnaireNodeReference CreateForMacro(Guid macroId)
            => new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Macro, macroId);

        public static QuestionnaireNodeReference CreateForLookupTable(Guid lookupTableId)
            => new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.LookupTable, lookupTableId);

        public static QuestionnaireNodeReference CreateForAttachment(Guid attachmentId)
            => new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Attachment, attachmentId);

        public static QuestionnaireNodeReference CreateForStaticText(Guid staticTextId)
            => new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.StaticText, staticTextId);

        public static QuestionnaireNodeReference CreateForTranslation(Guid translationId)
            => new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Translation, translationId);

        public static QuestionnaireNodeReference CreateFrom(IComposite entity)
        {
            QuestionnaireNodeReference result;

            if (entity is IVariable)
                result = QuestionnaireNodeReference.CreateForVariable(entity.PublicKey);
            else if (entity is IGroup)
                result = ((IGroup) entity).IsRoster
                    ? QuestionnaireNodeReference.CreateForRoster(entity.PublicKey)
                    : QuestionnaireNodeReference.CreateForGroup(entity.PublicKey);
            else if (entity is IQuestion)
                result = QuestionnaireNodeReference.CreateForQuestion(entity.PublicKey);
            else
                result = QuestionnaireNodeReference.CreateForStaticText(entity.PublicKey);

            return result;
        }
    }
}