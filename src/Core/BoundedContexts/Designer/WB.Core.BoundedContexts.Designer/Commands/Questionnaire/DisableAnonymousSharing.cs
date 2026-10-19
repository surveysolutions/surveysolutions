using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class DisableAnonymousSharing : QuestionnaireCommand
    {
        public DisableAnonymousSharing(Guid questionnaireId, Guid responsibleId, bool isResponsibleAdmin)
            : base(questionnaireId, responsibleId, isResponsibleAdmin)
        {
        }
    }
}


