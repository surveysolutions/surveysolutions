﻿using Main.Core.Commands.Questionnaire.Base;

namespace Main.Core.Commands.Questionnaire.Question
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewUpdateQuestion")]
    public class UpdateQuestionCommand : FullQuestionDataCommand
    {
        public UpdateQuestionCommand(Guid questionnaireId, Guid questionId,
            string title, QuestionType type, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions,
            Option[] options, Order optionsOrder, int? maxValue, Guid[] triggedGroupIds)
            : base(questionnaireId, questionId, title, type, alias, isMandatory, isFeatured, isHeaderOfPropagatableGroup,
                scope, condition, validationExpression, validationMessage, instructions, options, optionsOrder, maxValue, triggedGroupIds) { }
    }
}