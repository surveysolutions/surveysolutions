using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public class TranslationInstance: TranslationDto
    {
        public virtual Guid QuestionnaireId { get; set; }

        public virtual Guid Id { get; set; }

        public virtual TranslationInstance Clone()
        {
            return new TranslationInstance
            {
                QuestionnaireId = this.QuestionnaireId,
                QuestionnaireEntityId = this.QuestionnaireEntityId,
                TranslationId = this.TranslationId,
                TranslationIndex = this.TranslationIndex,
                Type = this.Type,
                Value = this.Value,
            };
        }

        internal class IdentityComparer : IEqualityComparer<TranslationInstance>
        {
            public bool Equals(TranslationInstance? x, TranslationInstance? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                
                return x.QuestionnaireId == y.QuestionnaireId &&
                       x.QuestionnaireEntityId == y.QuestionnaireEntityId &&
                       x.TranslationIndex == y.TranslationIndex &&
                       x.Type == y.Type;
            }
            
            public int GetHashCode(TranslationInstance obj)
            {
                unchecked
                {
                    var hashCode = (int)obj.Type;
                    hashCode = (hashCode * 397) ^ obj.QuestionnaireId.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.QuestionnaireEntityId.GetHashCode();
                    hashCode = (hashCode * 397) ^ (obj.TranslationIndex?.GetHashCode() ?? 0);
                    return hashCode;
                }
            }
        }
    }
}
