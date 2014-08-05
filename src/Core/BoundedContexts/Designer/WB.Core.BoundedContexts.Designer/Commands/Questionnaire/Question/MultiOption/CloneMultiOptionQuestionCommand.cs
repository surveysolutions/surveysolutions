using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.MultiOption
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "CloneMultiOptionQuestion")]
    public class CloneMultiOptionQuestionCommand : AbstractCloneQuestionCommand
    {
        public CloneMultiOptionQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            Guid parentGroupId,
            Guid sourceQuestionId,
            int targetIndex,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            string enablementCondition,
            string instructions,
            Guid responsibleId,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            Option[] options,
            Guid? linkedToQuestionId,
            bool areAnswersOrdered,
            int? maxAllowedAnswers)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions,
                parentGroupId: parentGroupId,
                sourceQuestionId: sourceQuestionId, targetIndex: targetIndex,variableLabel:variableLabel)
        {
            this.Scope = scope;
            this.ValidationMessage = validationMessage;
            this.ValidationExpression = validationExpression;
            this.Options = options;
            this.LinkedToQuestionId = linkedToQuestionId;
            this.AreAnswersOrdered = areAnswersOrdered;
            this.MaxAllowedAnswers = maxAllowedAnswers;
        }

        public QuestionScope Scope { get; set; }

        public string ValidationMessage { get; set; }

        public string ValidationExpression { get; set; }

        public bool AreAnswersOrdered { get; set; }

        public Guid? LinkedToQuestionId { get; set; }

        public int? MaxAllowedAnswers { get; set; }

        public Option[] Options { get; set; }
    }
}