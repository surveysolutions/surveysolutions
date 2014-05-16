using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_create_interview_with_3_fixed_rosters_one_inside_other_and_1_separate_fixed_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            topFixedRosterId = Guid.Parse("11111111111111111111111111111111");
            nestedFixedRosterId = Guid.Parse("21111111111111111111111111111111");
            separateFixedRosterId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new Group("top level fixed group")
                {
                    PublicKey = topFixedRosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "1", "2" },
                    Children = new List<IComposite>
                    {
                        new Group("nested fixed group")
                        {
                            PublicKey = nestedFixedRosterId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.FixedTitles,
                            RosterFixedTitles = new[] { "a", "b" }
                        }
                    }
                },
                new Group("separate fixed group")
                {
                    PublicKey = separateFixedRosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "I", "II" }
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview = CreateInterview(questionnaireId: questionnaireId);

        It should_produce_one_event_roster_instance_added = () =>
            eventContext.GetEvents<RosterInstancesAdded>().Count().ShouldEqual(1);

        It should_put_8_instances_to_RosterInstancesAdded_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event=>@event.Instances.Length==8);

        It should_produce_one_event_rosters_title_changed =()=>
             eventContext.GetEvents<RosterInstancesTitleChanged>().Count().ShouldEqual(1);

        It should_put_8_instances_to_RosterInstancesTitleChanged_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event => @event.ChangedInstances.Length == 8);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid topFixedRosterId;
        private static Guid nestedFixedRosterId;
        private static Guid separateFixedRosterId;
        private static Guid questionnaireId;
    }
}
