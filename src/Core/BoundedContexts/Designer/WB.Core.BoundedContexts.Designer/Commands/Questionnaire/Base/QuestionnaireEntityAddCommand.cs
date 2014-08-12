using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class QuestionnaireEntityAddCommand : QuestionnaireEntityCommand
    {
        protected QuestionnaireEntityAddCommand(Guid questionnaireId, Guid entityId, Guid responsibleId, Guid parentId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.ParentId = parentId;
        }

        public Guid ParentId { get; set; }
    }
}