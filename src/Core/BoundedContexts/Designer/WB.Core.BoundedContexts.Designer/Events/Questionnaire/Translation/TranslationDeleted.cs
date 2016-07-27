using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Translation
{
    public class TranslationDeleted : QuestionnaireActiveEvent
    {
        public TranslationDeleted() { }

        public TranslationDeleted(Guid translationId, Guid responsibleId)
        {
            this.TranslationId = translationId;
            this.ResponsibleId = responsibleId;
        }

        public Guid TranslationId { get; set; }
    }
}