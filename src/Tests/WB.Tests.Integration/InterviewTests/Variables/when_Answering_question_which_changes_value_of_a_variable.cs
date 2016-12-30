﻿using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    internal class when_removing_asnwer_which_changes_value_of_a_variable_that_uses_in_substitution : InterviewTestsContext
    {
        Establish context = () =>
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: QuestionnaireId,
                children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(id: n1Id, variable: "n1"),
                    Create.NumericIntegerQuestion(id: n2Id, variable: "n2"),
                    Create.Variable(id: variableId, variableName: "v1", expression: "n1+n2"),
                    Create.NumericIntegerQuestion(id: n3Id, variable: "n3", title: "title with %v1%"),
                });

            interview = SetupStatefullInterview(questionnaire);
            interview.AnswerNumericIntegerQuestion(userId, n1Id, new decimal[0], DateTime.Now, 1);
            interview.AnswerNumericIntegerQuestion(userId, n2Id, new decimal[0], DateTime.Now, 2);
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.RemoveAnswer(n1Id, new decimal[0], userId, DateTime.Now);

        It should_raise_VariablesValuesChanged_event_for_the_variable = () =>
            interview.GetTitleText(Create.Identity(n3Id)).ShouldEqual("title with [...]");

        private static EventContext eventContext;
        private static StatefulInterview interview;
        private static readonly Guid QuestionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static readonly Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static readonly Guid n1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid n2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid n3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid variableId =  Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }

    internal class when_Answering_question_which_changes_value_of_a_variable : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            textQuetionId = Guid.Parse("21111111111111111111111111111111");
            variableId = Guid.Parse("22222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.TextQuestion(id: textQuetionId, variable: "txt"),
                    Create.Variable(id: variableId, variableName: "v1", expression: "txt.Length")
                });

            interview = SetupInterview(questionnaireDocument: questionnaire);
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.AnswerTextQuestion(userId, textQuetionId, new decimal[0], DateTime.Now, "Nastya");

        It should_raise_VariablesValuesChanged_event_for_the_variable = () =>
            eventContext.ShouldContainEvent<VariablesChanged>(@event
                => (long?) @event.ChangedVariables[0].NewValue == 6 && @event.ChangedVariables[0].Identity.Id== variableId);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid variableId;
        private static Guid textQuetionId;
    }
}