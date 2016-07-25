using System;
using NHibernate.Type;
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
                Value = this.Value
            };
        }
    }
}