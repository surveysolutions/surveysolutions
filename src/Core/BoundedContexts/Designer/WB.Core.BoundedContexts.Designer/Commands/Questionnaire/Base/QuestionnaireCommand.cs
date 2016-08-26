using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionnaireCommand : QuestionnaireCommandBase
    {
        protected QuestionnaireCommand(Guid questionnaireId, Guid responsibleId)
            : this(questionnaireId: questionnaireId, responsibleId: responsibleId, hasResponsibleAdminRights: false)
        {
        }

        protected QuestionnaireCommand(Guid questionnaireId, Guid responsibleId, bool hasResponsibleAdminRights)
            : base(responsibleId, hasResponsibleAdminRights)
        {
            this.QuestionnaireId = questionnaireId;
        }

        public Guid QuestionnaireId { get; private set; }
    }
}