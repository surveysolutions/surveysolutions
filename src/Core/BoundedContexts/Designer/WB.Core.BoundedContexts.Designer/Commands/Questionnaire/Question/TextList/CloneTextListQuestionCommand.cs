using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.TextList
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "CloneTextListQuestion")]
    public class CloneTextListQuestionCommand : AbstractCloneQuestionCommand
    {
        public CloneTextListQuestionCommand(Guid questionnaireId, Guid questionId, Guid parentGroupId, Guid sourceQuestionId, int targetIndex,
            string title, string variableName, string variableLabel, bool isMandatory, string enablementCondition, string instructions, Guid responsibleId,
            int? maxAnswerCount)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions, parentGroupId: parentGroupId,
                sourceQuestionId: sourceQuestionId, targetIndex: targetIndex, variableLabel:variableLabel)
        {
            this.MaxAnswerCount = maxAnswerCount;
        }

        public int? MaxAnswerCount { get; private set; }
    }
}
