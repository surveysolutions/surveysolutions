﻿using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.TextList
{
    [Serializable]
    public class AddTextListQuestionCommand : AbstractAddQuestionCommand
    {
        public AddTextListQuestionCommand(Guid questionnaireId, Guid questionId, Guid parentGroupId, string title, string variableName, string variableLabel,
            bool isMandatory, string enablementCondition, string instructions, Guid responsibleId, int? maxAnswerCount)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions, parentGroupId: parentGroupId, variableLabel:variableLabel)
        {
            this.MaxAnswerCount = maxAnswerCount;
        }

        public int? MaxAnswerCount { get; private set; }
    }
}
