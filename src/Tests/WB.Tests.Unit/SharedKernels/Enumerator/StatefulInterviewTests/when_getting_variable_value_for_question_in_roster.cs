using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_variable_value_for_question_in_roster : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            expectedVariableValue = 555;
            questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            variableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var variableRosterVector = new[] {0m};

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(new []
            {
                Create.Entity.FixedRoster(fixedTitles: new [] { Create.Entity.FixedTitle(0, "fixed")}, children: new []
                {
                    Create.Entity.Variable(variableId)
                })
            });

            interview = Setup.StatefulInterview(questionnaireDocument);
            interview.Apply(Create.Event.InterviewSynchronized(
                Create.Entity.InterviewSynchronizationDto(variables: new Dictionary<InterviewItemId, object>
                {
                    {new InterviewItemId(variableId, variableRosterVector), expectedVariableValue}
                })));
        };

        Because of = () => actualVariableValue = interview.GetVariableValueByOrDeeperRosterLevel(variableId, new []{0m, 1m});

        It should_reduce_roster_vector_to_find_target_variable_value = () => actualVariableValue.ShouldEqual(expectedVariableValue);

        private static StatefulInterview interview;
        private static Guid variableId;
        private static int expectedVariableValue;
        private static object actualVariableValue;
        private static Guid questionnaireId;
    }
}

