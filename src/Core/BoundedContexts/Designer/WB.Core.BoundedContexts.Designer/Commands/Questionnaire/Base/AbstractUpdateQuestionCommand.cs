﻿using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractUpdateQuestionCommand : QuestionCommand
    {
        protected AbstractUpdateQuestionCommand(Guid responsibleId,
            Guid questionnaireId,
            Guid questionId, string title, string variableName, bool isMandatory, string enablementCondition, string instructions)
            : base(questionnaireId, questionId, responsibleId)
        {
            Title = title;
            VariableName = variableName;
            IsMandatory = isMandatory;
            this.EnablementCondition = enablementCondition;
            Instructions = instructions;
        }

        public string Title { get; private set; }
        public string VariableName { get; private set; }
        public bool IsMandatory { get; private set; }
        public string EnablementCondition { get; set; }
        public string Instructions { get; private set; }
    }
}