using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateMultimediaQuestionCommand: AbstractUpdateQuestionCommand
    {
        public UpdateMultimediaQuestionCommand(Guid questionnaireId, Guid questionId, string title, string variableName,
            string variableLabel,
            string enablementCondition, string instructions, Guid responsibleId,
            QuestionScope scope)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, enablementCondition: enablementCondition, instructions: instructions,
                variableLabel: variableLabel)
        {
            this.Scope = scope;
        }

        public QuestionScope Scope { get; set; }
    }
}
