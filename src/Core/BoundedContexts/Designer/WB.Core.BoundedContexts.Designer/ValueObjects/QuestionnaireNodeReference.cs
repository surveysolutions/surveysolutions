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
    public class QuestionnaireEntityReference
    {
        public QuestionnaireEntityReference(QuestionnaireVerificationReferenceType type, Guid id)
        {
            Type = type;
            Id = id;
            ItemId = id.FormatGuid();
        }

        public QuestionnaireVerificationReferenceType Type { get; private set; }
        public Guid Id { get; private set; }
        public string ItemId { get; private set; }
        public int? IndexOfEntityInProperty { get; set; }
        public Guid? ChapterId { get; set; }
        public QuestionnaireVerificationReferenceProperty Property { get; set; }

        protected bool Equals(QuestionnaireEntityReference other)
            => Id.Equals(other.Id)
            && Type == other.Type
            && IndexOfEntityInProperty == other.IndexOfEntityInProperty;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((QuestionnaireEntityReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (int)Type;
            }
        }

        public static QuestionnaireEntityReference CreateForQuestionnaire(Guid guestionnaireId)
            => new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Questionnaire, guestionnaireId);

        public static QuestionnaireEntityReference CreateForGroup(Guid groupId)
            => new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Group, groupId);

        public static QuestionnaireEntityReference CreateForVariable(Guid variableId)
            => new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Variable, variableId);

        public static QuestionnaireEntityReference CreateForRoster(Guid rosterId)
            => new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Roster, rosterId);

        public static QuestionnaireEntityReference CreateForQuestion(Guid questionId)
            => new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Question, questionId);

        public static QuestionnaireEntityReference CreateForMacro(Guid macroId)
            => new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Macro, macroId);

        public static QuestionnaireEntityReference CreateForLookupTable(Guid lookupTableId)
            => new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.LookupTable, lookupTableId);

        public static QuestionnaireEntityReference CreateForAttachment(Guid attachmentId)
            => new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Attachment, attachmentId);

        public static QuestionnaireEntityReference CreateForStaticText(Guid staticTextId)
            => new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.StaticText, staticTextId);

        public static QuestionnaireEntityReference CreateForTranslation(Guid translationId)
            => new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Translation, translationId);

        public static QuestionnaireEntityReference CreateFrom(IQuestionnaireEntity entity,
            QuestionnaireVerificationReferenceProperty property = QuestionnaireVerificationReferenceProperty.None,
            int? indexOfEntityInProperty = null)
        {
            QuestionnaireEntityReference result;

            if (entity is IVariable)
                result = CreateForVariable(entity.PublicKey);
            else if (entity is IGroup)
            {
                result = ((IGroup) entity).IsRoster
                    ? CreateForRoster(entity.PublicKey)
                    : CreateForGroup(entity.PublicKey);
            }
            else if (entity is IQuestion)
                result = CreateForQuestion(entity.PublicKey);
            else
                result = CreateForStaticText(entity.PublicKey);


            var section = entity;
            while (true)
            {
                IComposite grandParent = section.GetParent();
                if (grandParent?.GetParent() == null)
                {
                    break;
                }
                else
                {
                    section = grandParent;
                }
            }

            result.ChapterId = section.PublicKey;
            result.Property = property;
            result.IndexOfEntityInProperty = indexOfEntityInProperty;

            return result;
        }
    }
}
