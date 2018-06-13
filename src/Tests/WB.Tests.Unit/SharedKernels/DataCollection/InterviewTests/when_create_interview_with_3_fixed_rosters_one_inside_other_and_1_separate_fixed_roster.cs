using System;
using System.Linq;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_create_interview_with_3_fixed_rosters_one_inside_other_and_1_separate_fixed_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(

                Create.Entity.FixedRoster(rosterId: Guid.Parse("11111111111111111111111111111111"),
                    title: "top level fixed group", obsoleteFixedTitles: new[] {"1", "2"}, children: new[]
                    {
                        Create.Entity.FixedRoster(rosterId: Guid.Parse("21111111111111111111111111111111"),
                            title: "nested fixed group", obsoleteFixedTitles: new[] {"a", "b"}, children: new[]
                            {
                                Create.Entity.FixedRoster(rosterId: Guid.Parse("31111111111111111111111111111111"),
                                    title: "nested fixed subgroup", obsoleteFixedTitles: new[] {"x", "y"})
                            })
                    }),
                Create.Entity.FixedRoster(rosterId: Guid.Parse("22222222222222222222222222222222"),
                    title: "separate fixed group", obsoleteFixedTitles: new[] {"I", "II"}));

            questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaire, 1));

            eventContext = new EventContext();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
            CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

        [NUnit.Framework.Test] public void should_produce_one_event_roster_instance_added () =>
            eventContext.GetEvents<RosterInstancesAdded>().Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_put_16_instances_to_RosterInstancesAdded_event () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event=>@event.Instances.Length == 16);

        [NUnit.Framework.Test] public void should_produce_one_event_rosters_title_changed () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_put_16_instances_to_RosterInstancesTitleChanged_event () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event => @event.ChangedInstances.Length == 16);

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static IQuestionnaireStorage questionnaireRepository;
    }
}
