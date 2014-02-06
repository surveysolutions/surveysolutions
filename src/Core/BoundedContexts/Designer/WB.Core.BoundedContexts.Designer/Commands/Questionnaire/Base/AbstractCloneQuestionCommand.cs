using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractCloneQuestionCommand : AbstractAddQuestionCommand
    {
        protected AbstractCloneQuestionCommand(Guid responsibleId, Guid questionnaireId, Guid questionId, string title, string variableName,
            bool isMandatory, string condition, string instructions, Guid groupId, Guid sourceQuestionId, int targetIndex)
            : base(
                questionnaireId: questionnaireId, questionId: questionId, responsibleId: responsibleId, title: title,
                variableName: variableName, isMandatory: isMandatory, condition: condition, instructions: instructions, groupId: groupId)
        {
            this.SourceQuestionId = sourceQuestionId;
            this.TargetIndex = targetIndex;
        }

        public Guid SourceQuestionId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}
