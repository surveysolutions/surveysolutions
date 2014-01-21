using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "CloneTextListQuestion")]
    public class CloneTextListQuestionCommand : AbstractListQuestionCommand
    {
        public CloneTextListQuestionCommand(Guid questionnaireId, Guid questionId, Guid groupId, Guid sourceQuestionId, int targetIndex,
            string title, string alias, bool isMandatory, bool isFeatured, QuestionScope scope, string condition, 
            string validationExpression, string validationMessage, string instructions, Guid responsibleId, int? countOfDecimalPlaces)
            : base(questionnaireId, questionId, title, alias, isMandatory, isFeatured, scope, condition, 
                   validationExpression, validationMessage, instructions, responsibleId, countOfDecimalPlaces)
        {
            this.GroupId = groupId;
            this.SourceQuestionId = sourceQuestionId;
            this.TargetIndex = targetIndex;
        }

        public Guid GroupId { get; private set; }
        public Guid SourceQuestionId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}
