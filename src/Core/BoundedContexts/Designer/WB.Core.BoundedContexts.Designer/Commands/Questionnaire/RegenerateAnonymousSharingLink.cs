using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class RegenerateAnonymousSharingLink : QuestionnaireCommand
    {
        public RegenerateAnonymousSharingLink(Guid questionnaireId, Guid responsibleId, bool isResponsibleAdmin)
            : base(questionnaireId, responsibleId, isResponsibleAdmin)
        {
        }
    }
}


