using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    public class UpdateValidatableQuestionCommand : AbstractUpdateQuestionCommand
    {
        public UpdateValidatableQuestionCommand(Guid responsibleId, Guid questionnaireId, Guid questionId, 
            CommonQuestionParameters commonQuestionParameters, List<ValidationCondition> validationConditions)
            : base(responsibleId, questionnaireId, questionId, commonQuestionParameters)
        {
            this.ValidationConditions = validationConditions;
            this.ValidationConditions
                .ForEach(x => x.Message = CommandUtils.SanitizeHtml(x.Message, removeAllTags: true));
        }

        public List<ValidationCondition> ValidationConditions { get; set; }
    }
}
