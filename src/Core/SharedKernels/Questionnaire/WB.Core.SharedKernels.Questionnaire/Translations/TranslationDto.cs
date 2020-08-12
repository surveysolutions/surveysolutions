using System;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public class TranslationDto
    {
        public virtual TranslationType Type { get; set; }

        public virtual Guid QuestionnaireEntityId { get; set; }

        public virtual string? TranslationIndex { get; set; }

        public virtual Guid TranslationId { get; set; }

        public virtual string? Value { get; set; }
    }
}
