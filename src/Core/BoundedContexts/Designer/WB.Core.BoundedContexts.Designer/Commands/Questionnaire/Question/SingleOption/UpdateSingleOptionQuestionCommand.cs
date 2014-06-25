using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.SingleOption
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "UpdateSingleOptionQuestion")]
    public class UpdateSingleOptionQuestionCommand : AbstractUpdateQuestionCommand
    {
        public UpdateSingleOptionQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            string enablementCondition,
            string instructions,
            Guid responsibleId,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            bool isPreFilled,
            Option[] options,
            Guid? linkedToQuestionId)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions, variableLabel:variableLabel)
        {
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
            this.ValidationMessage = validationMessage;
            this.ValidationExpression = validationExpression;
            this.Options = options;
            this.LinkedToQuestionId = linkedToQuestionId;
        }

        public QuestionScope Scope { get; set; }

        public string ValidationMessage { get; set; }

        public string ValidationExpression { get; set; }

        public bool IsPreFilled { get; set; }

        public Guid? LinkedToQuestionId { get; set; }

        public Option[] Options { get; set; }
    }
}