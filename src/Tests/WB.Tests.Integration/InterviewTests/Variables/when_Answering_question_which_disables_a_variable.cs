using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    internal class when_Answering_question_which_disables_a_variable : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            textQuetionId = Guid.Parse("21111111111111111111111111111111");
            variableId = Guid.Parse("22222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Create.Entity.TextQuestion(textQuetionId, variable: "txt"),
                Create.Entity.Group(Guid.NewGuid(), "Group X", null, "txt!=\"Nastya\"", false, new[]
                {
                    Create.Entity.Variable(variableId, variableName: "v1", expression: "txt.Length")
                })
            });

            interview = SetupInterview(questionnaire);
            eventContext = new EventContext();

            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
            interview.AnswerTextQuestion(userId, textQuetionId, new decimal[0], DateTime.Now, "Nastya");

        [NUnit.Framework.Test] public void should_not_raise_VariablesValuesChanged_event_for_the_variable_with_value_equal_to_null () =>
           eventContext.ShouldNotContainEvent<VariablesChanged>();

        [NUnit.Framework.Test] public void should_raise_VariablesDisabled_event_for_the_variable () =>
           eventContext.ShouldContainEvent<VariablesDisabled>(@event => @event.Variables[0].Id== variableId);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid variableId;
        private static Guid textQuetionId;
    }
}
