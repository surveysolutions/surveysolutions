using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_and_questionnaire_has_fixed_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            answersToFeaturedQuestions = new Dictionary<Guid, object>();
            fixedRosterId = Guid.Parse("22220000FFFFFFFFFFFFFFFFFFFFFFFF");

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireIdentity(questionnaireId, 1),
                Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
                {
                    Create.Entity.Roster(rosterId: fixedRosterId, variable: "rosterFixed",
                        fixedRosterTitles: new[]
                        {
                            new FixedRosterTitle(0, "Title 1"),
                            new FixedRosterTitle(1, "Title 2"),
                            new FixedRosterTitle(2, "Title 3")
                        },
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles)
                }));

            eventContext = new EventContext();

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            interview.CreateInterview(questionnaireId, 1, supervisorId, answersToFeaturedQuestions, DateTime.Now, userId);

        It should_raise_RosterInstancesAdded_event_with_3_instances = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Count().ShouldEqual(3);

        It should_raise_RosterInstancesTitleChanged_event_with_3_instances = () =>
          eventContext.GetEvent<RosterInstancesAdded>().Instances.Count().ShouldEqual(3);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static Dictionary<Guid, object> answersToFeaturedQuestions;
        private static Guid supervisorId;
        private static Guid fixedRosterId;
        private static Interview interview;
    }
}