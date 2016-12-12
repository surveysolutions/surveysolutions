using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations
{
    [Serializable]
    public class AddOrUpdateTranslation : QuestionnaireCommand
    {
        public AddOrUpdateTranslation(
            Guid questionnaireId,
            Guid responsibleId,
            Guid translationId,
            string name,
            Guid? oldTranslationId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.TranslationId = translationId;
            this.Name = name;
            this.OldTranslationId = oldTranslationId;
        }

        public Guid? OldTranslationId { get; set; }
        public Guid TranslationId { get; set; }
        public string Name { get; set; }
    }
}