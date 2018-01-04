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
    }
}