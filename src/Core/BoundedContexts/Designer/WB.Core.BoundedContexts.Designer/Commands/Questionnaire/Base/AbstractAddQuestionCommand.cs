using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractAddQuestionCommand : AbstractUpdateQuestionCommand
    {
        protected AbstractAddQuestionCommand(Guid responsibleId, Guid questionnaireId, Guid questionId, string title, string variableName,
            bool isMandatory, string condition, string instructions, Guid groupId)
            : base(
                questionnaireId: questionnaireId, questionId: questionId, responsibleId: responsibleId, title: title,
                variableName: variableName, isMandatory: isMandatory, condition: condition, instructions: instructions)
        {
            this.GroupId = groupId;
        }

        public Guid GroupId { get; private set; }
    }
}