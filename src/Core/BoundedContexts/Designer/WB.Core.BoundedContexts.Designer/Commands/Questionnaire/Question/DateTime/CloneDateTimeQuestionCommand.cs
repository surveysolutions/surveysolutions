﻿using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.DateTime
{
    [Serializable]
    public class CloneDateTimeQuestionCommand : AbstractCloneQuestionCommand
    {
        public CloneDateTimeQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            Guid parentGroupId,
            Guid sourceQuestionId,
            int targetIndex,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            string enablementCondition,
            string instructions,
            Guid responsibleId,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            bool isPreFilled)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions,
                parentGroupId: parentGroupId,
                sourceQuestionId: sourceQuestionId, targetIndex: targetIndex,variableLabel:variableLabel)
        {
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
            this.ValidationMessage = CommandUtils.SanitizeHtml(validationMessage, removeAllTags: true);
            this.ValidationExpression = validationExpression;
        }

        public QuestionScope Scope { get; set; }

        public string ValidationMessage { get; set; }

        public string ValidationExpression { get; set; }

        public bool IsPreFilled { get; set; }
    }
}