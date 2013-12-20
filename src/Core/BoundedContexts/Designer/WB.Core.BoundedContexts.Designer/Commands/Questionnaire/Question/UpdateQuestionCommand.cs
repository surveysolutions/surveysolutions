﻿using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "NewUpdateQuestion")]
    public class UpdateQuestionCommand : FullQuestionDataCommand
    {
        public UpdateQuestionCommand(Guid questionnaireId, Guid questionId,
            string title, QuestionType type, string alias, bool isMandatory, bool isFeatured,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions,
            Option[] options, Order optionsOrder, Guid responsibleId, Guid? linkedToQuestionId, bool areAnswersOrdered, int? maxAllowedAnswers)

            : base(questionnaireId, questionId, title, type, alias, isMandatory, isFeatured,
                scope, condition, validationExpression, validationMessage, instructions, options, optionsOrder, responsibleId, linkedToQuestionId, areAnswersOrdered, maxAllowedAnswers) { }
    }
}