using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractQuestionCommand : QuestionCommand
    {
        protected AbstractQuestionCommand(Guid questionnaireId, Guid questionId,
            string title, QuestionType type, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions,Guid responsibleId)
            : base(questionnaireId, questionId, responsibleId)
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
        public string Title { get; private set; }

        public QuestionType Type { get; private set; }

        public string Alias { get; private set; }

        public bool IsMandatory { get; private set; }

        public bool IsFeatured { get; private set; }

        public bool IsHeaderOfPropagatableGroup { get; private set; }

        public QuestionScope Scope { get; private set; }

        public string Condition { get; set; }

        public string ValidationExpression { get; set; }

        public string ValidationMessage { get; private set; }

        public string Instructions { get; private set; }
    }
}
