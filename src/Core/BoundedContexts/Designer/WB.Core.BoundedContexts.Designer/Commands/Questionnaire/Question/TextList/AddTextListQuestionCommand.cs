using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.TextList
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "AddTextListQuestion")]
    public class AddTextListQuestionCommand : AbstractAddQuestionCommand
    {
        public AddTextListQuestionCommand(Guid questionnaireId, Guid questionId, Guid parentGroupId, string title, string variableName,
            bool isMandatory, string condition, string instructions, Guid responsibleId, int? maxAnswerCount)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, condition: condition, instructions: instructions, parentGroupId: parentGroupId)
        {
            this.MaxAnswerCount = maxAnswerCount;
        }

        public int? MaxAnswerCount { get; private set; }
    }
}
