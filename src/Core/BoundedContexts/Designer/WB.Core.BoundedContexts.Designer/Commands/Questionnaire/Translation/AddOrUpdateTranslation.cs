using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translation
{
    [Serializable]
    public class AddOrUpdateTranslation : QuestionnaireCommand
    {
        public AddOrUpdateTranslation(
            Guid questionnaireId,
            Guid responsibleId,
            Guid translationId,
            string name)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.TranslationId = translationId;
            this.Name = name;
        }

        public Guid TranslationId { get; set; }
        public string Name { get; set; }
    }
}