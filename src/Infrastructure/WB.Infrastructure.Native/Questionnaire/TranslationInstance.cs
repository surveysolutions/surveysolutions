using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Enumerator.Native.Questionnaire
{
    public class TranslationInstance : TranslationDto
    {
        public virtual int Id { get; set; }

        public virtual QuestionnaireIdentity QuestionnaireId { get; set; }

        public virtual TranslationInstance Clone()
        {
            return new TranslationInstance
            {
                QuestionnaireId = this.QuestionnaireId,
                TranslationId = this.TranslationId,
                QuestionnaireEntityId = this.QuestionnaireEntityId,
                TranslationIndex = this.TranslationIndex,
                Type = this.Type,
                Value = this.Value
            };
        }

        internal class IdentityComparer : IEqualityComparer<TranslationInstance>
        {
            public bool Equals(TranslationInstance x, TranslationInstance y)
                => x.QuestionnaireId == y.QuestionnaireId &&
                   x.QuestionnaireEntityId == y.QuestionnaireEntityId &&
                   x.TranslationIndex == y.TranslationIndex &&
                   x.Type == y.Type;

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
