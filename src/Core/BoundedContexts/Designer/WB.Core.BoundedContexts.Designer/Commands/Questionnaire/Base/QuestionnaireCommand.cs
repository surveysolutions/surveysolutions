using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionnaireCommand : QuestionnaireCommandBase
    {
        protected QuestionnaireCommand(Guid questionnaireId, Guid responsibleId)
            : this(responsibleId, questionnaireId, false)
        {;
        }

        protected QuestionnaireCommand(Guid questionnaireId, Guid responsibleId, bool hasResponsibleAdminRights)
            : base(responsibleId, hasResponsibleAdminRights)
        {
            this.QuestionnaireId = questionnaireId;
        }

        public Guid QuestionnaireId { get; private set; }
    }
}