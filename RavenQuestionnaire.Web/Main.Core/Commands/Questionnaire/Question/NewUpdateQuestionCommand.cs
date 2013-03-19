namespace Main.Core.Commands.Questionnaire.Question
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewUpdateQuestion")]
    public class NewUpdateQuestionCommand : FullQuestionDataCommand
    {
        public NewUpdateQuestionCommand(Guid questionnaireId, Guid questionId, string title, QuestionType type, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup, QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions)
            : base(questionnaireId, questionId, title, type, alias, isMandatory, isFeatured, isHeaderOfPropagatableGroup, scope, condition, validationExpression, validationMessage, instructions) {}
    }
}