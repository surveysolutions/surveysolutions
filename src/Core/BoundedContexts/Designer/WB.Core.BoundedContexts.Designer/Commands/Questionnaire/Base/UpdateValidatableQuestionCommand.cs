using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public class UpdateValidatableQuestionCommand : AbstractUpdateQuestionCommand
    {
        public UpdateValidatableQuestionCommand(Guid responsibleId,
            Guid questionnaireId, 
            Guid questionId,
            string title,
            string variableName, 
            string variableLabel, 
            string enablementCondition, 
            string instructions,
            List<ValidationCondition> validationConditions)
            : base(responsibleId, 
                  questionnaireId, 
                  questionId, 
                  title, 
                  variableName, 
                  variableLabel, 
                  enablementCondition, 
                  instructions)
        {
            this.ValidationConditions = validationConditions ?? new List<ValidationCondition>();

            foreach (var validationCondition in validationConditions)
            {
                validationCondition.Message = CommandUtils.SanitizeHtml(validationCondition.Message, removeAllTags: true);
            }
        }

        public List<ValidationCondition> ValidationConditions { get; private set; }
    }
}