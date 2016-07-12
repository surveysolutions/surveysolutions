using System;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public class TranslationInstance
    {
        public virtual Guid QuestionnaireId { get; set; }

        public virtual TranslationType Type { get; set; }

        public virtual Guid QuestionnaireEntityId { get; set; }

        public virtual string TranslationIndex { get; set; }

        public virtual string Language { get; set; }

        public virtual string Translation { get; set; }

        public virtual int Id { get; set; }
    }
}