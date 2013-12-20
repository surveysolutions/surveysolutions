﻿using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractQuestionCommand : QuestionCommand
    {
        protected AbstractQuestionCommand(Guid questionnaireId, Guid questionId,
            string title, string alias, bool isMandatory, bool isFeatured,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions,Guid responsibleId)
            : base(questionnaireId, questionId, responsibleId)
        {
            this.Title = title;
            this.Alias = alias;
            this.IsMandatory = isMandatory;
            this.IsFeatured = isFeatured;
            this.Scope = scope;
            this.Condition = condition;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.Instructions = instructions;
        }
        public string Title { get; private set; }

        public string Alias { get; private set; }

        public bool IsMandatory { get; private set; }

        public bool IsFeatured { get; private set; }

        public QuestionScope Scope { get; private set; }

        public string Condition { get; set; }

        public string ValidationExpression { get; set; }

        public string ValidationMessage { get; private set; }

        public string Instructions { get; private set; }
    }
}
