using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.TextList
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "UpdateTextListQuestion")]
    public class UpdateTextListQuestionCommand : AbstractUpdateQuestionCommand
    {
        public UpdateTextListQuestionCommand(Guid questionnaireId, Guid questionId, string title, string variableName, string variableLabel, bool isMandatory,
            string enablementCondition, string instructions, Guid responsibleId, int? maxAnswerCount)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions, variableLabel:variableLabel)
        {
            this.MaxAnswerCount = maxAnswerCount;
        }

        public int? MaxAnswerCount { get; private set; }
    }
}
