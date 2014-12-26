﻿using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests.Clone
{
    public class when_cloning_numeric_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = CreateQuestionnaire(responsibleId);
            var groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId });
            sourceQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var newQuestionAdded = new NumericQuestionAdded
            {
                PublicKey = sourceQuestionId,
                QuestionText = "text",
                StataExportCaption = "varrr",
                VariableLabel = "varlabel",
                IsInteger = true,
                MaxAllowedValue = 50,
                GroupPublicKey = groupId
            };
            questionnaire.Apply(newQuestionAdded);

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.CloneQuestionById(sourceQuestionId, responsibleId, targetId);

        It should_clone_IsInteger_value = () => eventContext.ShouldContainEvent<QuestionCloned>(x => x.PublicKey == targetId && x.IsInteger.GetValueOrDefault());

        It should_clone_MaxAllowedValue_property = () => eventContext.ShouldContainEvent<QuestionCloned>(x => x.PublicKey == targetId && x.MaxValue == 50);

        static Questionnaire questionnaire;
        static Guid sourceQuestionId;
        static EventContext eventContext;
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}

