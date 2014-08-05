using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractAddQuestionCommand : AbstractUpdateQuestionCommand
    {
        protected AbstractAddQuestionCommand(Guid responsibleId, Guid questionnaireId, Guid questionId, string title, string variableName, string variableLabel,
            bool isMandatory, string enablementCondition, string instructions, Guid parentGroupId)
            : base(
                questionnaireId: questionnaireId, questionId: questionId, responsibleId: responsibleId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions,variableLabel: variableLabel)
        {
            this.ParentGroupId = parentGroupId;
        }

        public Guid ParentGroupId { get; private set; }
    }
}