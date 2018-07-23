using System;
using System.Collections.Generic;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_and_questionnaire_has_group_with_custom_enablement_condition_and_group_has_propagatable_ancestor_group : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            answersTime = new DateTime(2013, 09, 01);

            Guid groupId = Guid.Parse("22220000FFFFFFFFFFFFFFFFFFFFFFFF");
            Guid parentPropagatableGroupId = Guid.Parse("22220000AAAAAAAAAAAAAAAAAAAAAAAA");

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.GetRostersFromTopToSpecifiedGroup(groupId) == new[] { parentPropagatableGroupId });

            eventContext = new EventContext();

            command = Create.Command.CreateInterview(questionnaireId, 1, supervisorId,
                new List<InterviewAnswer>(), userId: userId);
            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.CreateInterview(command);

        [NUnit.Framework.Test] public void should_not_raise_GroupDisabled_event () =>
            eventContext.ShouldNotContainEvent<GroupsDisabled>();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Interview interview;
        private static CreateInterview command;
    }
}
