using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "UpdateTextListQuestion")]
    public class UpdateTextListQuestionCommand : AbstractListQuestionCommand
    {
        public UpdateTextListQuestionCommand(Guid questionnaireId, Guid questionId, string title, string alias, bool isMandatory, 
            bool isFeatured, QuestionScope scope, string condition, string validationExpression, string validationMessage, 
            string instructions, Guid responsibleId, int? maxAnswerCount)
            : base(questionnaireId, questionId, title, alias, isMandatory, isFeatured, scope, condition,
                validationExpression, validationMessage, instructions, responsibleId, maxAnswerCount){}
    }
}
