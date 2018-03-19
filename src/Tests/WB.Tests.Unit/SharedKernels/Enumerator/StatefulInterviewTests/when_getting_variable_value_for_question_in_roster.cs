using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_variable_value_for_question_in_roster : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            expectedVariableValue = 555;

            variableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var variableRosterVector = Create.RosterVector(0);

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(new []
            {
                Create.Entity.FixedRoster(fixedTitles: new [] { Create.Entity.FixedTitle(0, "fixed")}, children: new []
                {
                    Create.Entity.Variable(variableId)
                })
            });

            interview = Setup.StatefulInterview(questionnaireDocument);
            interview.Apply(Create.Event.VariablesChanged(Create.Entity.ChangedVariable(Create.Identity(variableId, variableRosterVector), expectedVariableValue)));
            BecauseOf();
        }

        private void BecauseOf() => actualVariableValue = interview.GetVariableValueByOrDeeperRosterLevel(variableId, Create.RosterVector(0, 1));

        [NUnit.Framework.Test] public void should_reduce_roster_vector_to_find_target_variable_value () => actualVariableValue.Should().Be(expectedVariableValue);

        private static StatefulInterview interview;
        private static Guid variableId;
        private static int expectedVariableValue;
        private static object actualVariableValue;
    }
}

