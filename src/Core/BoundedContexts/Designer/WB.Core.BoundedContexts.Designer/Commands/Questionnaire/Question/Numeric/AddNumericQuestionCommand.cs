using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Numeric
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "AddNumericQuestion")]
    public class AddNumericQuestionCommand : AbstractAddQuestionCommand
    {
        public AddNumericQuestionCommand(
            Guid questionnaireId, 
            Guid questionId, 
            Guid parentGroupId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory, 
            bool isPreFilled,
            QuestionScope scope, 
            string enablementCondition, 
            string validationExpression, 
            string validationMessage, 
            string instructions, 
            int? maxValue,
             Guid responsibleId,
            bool isInteger,
            int? countOfDecimalPlaces)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions,
                parentGroupId: parentGroupId,variableLabel:variableLabel)
        {
            this.MaxValue = maxValue;
            this.IsInteger = isInteger;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
            this.ValidationMessage = validationMessage;
            this.ValidationExpression = validationExpression;
        }

        public int? MaxValue { get; private set; }

        public bool IsInteger { get; private set; }

        public int? CountOfDecimalPlaces { get; private set; }

        public QuestionScope Scope { get; set; }

        public string ValidationMessage { get; set; }

        public string ValidationExpression { get; set; }

        public bool IsPreFilled { get; set; }
    }
}
