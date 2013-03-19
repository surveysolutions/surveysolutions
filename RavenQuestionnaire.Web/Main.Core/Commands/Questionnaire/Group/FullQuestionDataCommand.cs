namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Entities.SubEntities;

    public abstract class FullQuestionDataCommand : QuestionCommand
    {
        protected FullQuestionDataCommand(Guid questionnaireId, Guid questionId, string title, QuestionType type, string @alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup, QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions)
            : base(questionnaireId, questionId)
        {
            this.Title = title;
            this.Type = type;
            this.Alias = alias;
            this.IsMandatory = isMandatory;
            this.IsFeatured = isFeatured;
            this.IsHeaderOfPropagatableGroup = isHeaderOfPropagatableGroup;
            this.Scope = scope;
            this.Condition = condition;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.Instructions = instructions;
        }

        public string Title { get; set; }

        public QuestionType Type { get; set; }

        public string Alias { get; set; }

        public bool IsMandatory { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsHeaderOfPropagatableGroup { get; set; }

        public QuestionScope Scope { get; set; }

        public string Condition { get; set; }

        public string ValidationExpression { get; set; }

        public string ValidationMessage { get; set; }

        public string Instructions { get; set; }
    }
}