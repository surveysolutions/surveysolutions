using Main.Core.Commands.Questionnaire.Base;

namespace Main.Core.Commands.Questionnaire.Question
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

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