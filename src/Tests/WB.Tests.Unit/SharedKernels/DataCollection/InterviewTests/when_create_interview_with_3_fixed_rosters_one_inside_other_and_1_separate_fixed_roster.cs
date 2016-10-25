using System;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_create_interview_with_3_fixed_rosters_one_inside_other_and_1_separate_fixed_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(

                Create.Entity.FixedRoster(rosterId: Guid.Parse("11111111111111111111111111111111"),
                    title: "top level fixed group", fixedTitles: new[] {"1", "2"}, children: new[]
                    {
                        Create.Entity.FixedRoster(rosterId: Guid.Parse("21111111111111111111111111111111"),
                            title: "nested fixed group", fixedTitles: new[] {"a", "b"}, children: new[]
                            {
                                Create.Entity.FixedRoster(rosterId: Guid.Parse("31111111111111111111111111111111"),
                                    title: "nested fixed subgroup", fixedTitles: new[] {"x", "y"})
                            })
                    }),
                Create.Entity.FixedRoster(rosterId: Guid.Parse("22222222222222222222222222222222"),
                    title: "separate fixed group", fixedTitles: new[] {"I", "II"}));

            questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

        It should_produce_one_event_roster_instance_added = () =>
            eventContext.GetEvents<RosterInstancesAdded>().Count().ShouldEqual(1);

        It should_put_16_instances_to_RosterInstancesAdded_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event=>@event.Instances.Length == 16);

        It should_produce_one_event_rosters_title_changed =()=>
            eventContext.GetEvents<RosterInstancesTitleChanged>().Count().ShouldEqual(1);

        It should_put_16_instances_to_RosterInstancesTitleChanged_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event => @event.ChangedInstances.Length == 16);

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static IQuestionnaireStorage questionnaireRepository;
    }
}
