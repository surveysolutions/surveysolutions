using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations
{
    [Serializable]
    public class SetDefaultTranslation : QuestionnaireCommand
    {
        public SetDefaultTranslation(Guid questionnaireId, Guid responsibleId, Guid? translationId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.TranslationId = translationId;
        }

        public Guid? TranslationId { get; private set; }
    }
}