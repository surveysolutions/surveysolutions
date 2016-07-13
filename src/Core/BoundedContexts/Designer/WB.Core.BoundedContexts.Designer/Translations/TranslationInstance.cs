using System;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public class TranslationInstance: TranslationDto
    {
        public virtual Guid QuestionnaireId { get; set; }

        public virtual int Id { get; set; }
    }
}