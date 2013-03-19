namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewAddQuestion")]
    public class NewAddQuestionCommand : FullQuestionDataCommand
    {
        public NewAddQuestionCommand(Guid questionnaireId, Guid questionId, string title, QuestionType type, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup, QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions, Guid groupId)
            : base(questionnaireId, questionId, title, type, alias, isMandatory, isFeatured, isHeaderOfPropagatableGroup, scope, condition, validationExpression, validationMessage, instructions)
        {
            this.GroupId = groupId;
        }

        public Guid GroupId { get; set; }
    }
}