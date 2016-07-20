using System;
using NHibernate.Type;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public class TranslationInstance: TranslationDto
    {
        public virtual Guid QuestionnaireId { get; set; }

        public virtual Guid Id { get; set; }
    }
}