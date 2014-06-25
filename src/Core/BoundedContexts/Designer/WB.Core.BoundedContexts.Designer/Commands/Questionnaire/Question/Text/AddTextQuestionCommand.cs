using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Text
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "AddTextQuestion")]
    public class AddTextQuestionCommand : AbstractAddQuestionCommand
    {
        public AddTextQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            string enablementCondition,
            string instructions,
            Guid responsibleId,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            bool isPreFilled)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions,
                parentGroupId: parentGroupId, variableLabel:variableLabel)
        {
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
            this.ValidationMessage = validationMessage;
            this.ValidationExpression = validationExpression;
        }

        public QuestionScope Scope { get; set; }

        public string ValidationMessage { get; set; }

        public string ValidationExpression { get; set; }

        public bool IsPreFilled { get; set; }
    }
}
