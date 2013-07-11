using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewAddQuestion")]
    public class AddQuestionCommand : FullQuestionDataCommand
    {
        public AddQuestionCommand(Guid questionnaireId, Guid questionId, Guid groupId,
            string title, QuestionType type, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions,
            Option[] options, Order optionsOrder, int? maxValue, Guid[] triggedGroupIds)
            : base(questionnaireId, questionId, title, type, alias, isMandatory, isFeatured, isHeaderOfPropagatableGroup,
                scope, condition, validationExpression, validationMessage, instructions, options, optionsOrder, maxValue, triggedGroupIds)
        {
            this.GroupId = groupId;
        }

        public Guid GroupId { get; set; }
    }
}