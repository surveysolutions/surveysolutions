using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Translation
{
    public class TranslationUpdated : QuestionnaireActiveEvent
    {
        public TranslationUpdated() { }

        public TranslationUpdated(Guid translationId, string name, Guid responsibleId)
        {
            this.TranslationId = translationId;
            this.Name = name;
            this.ResponsibleId = responsibleId;
        }

        public Guid TranslationId { get; set; }

        public string Name { get; set; }
    }
}