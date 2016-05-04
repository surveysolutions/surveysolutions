using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_interviewer_entities : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            IPlainQuestionnaireRepository questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.GetChildEntityIds(selectedGroupIdentity.Id) == new ReadOnlyCollection<Guid>(new [] { staticTextId, prefilledTextQuestionId, interviewerTextQuetionId, rosterId, groupId })
                && _.IsRosterGroup(rosterId) == true
                && _.IsRosterGroup(groupId) == false
                && _.IsRosterGroup(staticTextId) == false
                && _.IsRosterGroup(prefilledTextQuestionId) == false
                && _.IsRosterGroup(interviewerTextQuetionId) == false
                && _.IsQuestion(staticTextId) == false
                && _.IsQuestion(groupId) == false
                && _.IsQuestion(prefilledTextQuestionId) == true
                && _.IsQuestion(interviewerTextQuetionId) == true
                && _.IsQuestion(rosterId) == false
                && _.IsInterviewierQuestion(staticTextId) == false
                && _.IsInterviewierQuestion(groupId) == false
                && _.IsInterviewierQuestion(prefilledTextQuestionId) == false
                && _.IsInterviewierQuestion(interviewerTextQuetionId) == true
                && _.IsInterviewierQuestion(rosterId) == false
                && _.GetRosterLevelForEntity(staticTextId) == 0
                && _.GetRosterLevelForEntity(groupId) == 0
                && _.GetRosterLevelForEntity(interviewerTextQuetionId) == 0
                && _.GetRosterLevelForGroup(rosterId) == 1
                && _.GetRostersFromTopToSpecifiedGroup(rosterId) == new [] { rosterId }
                && _.GetRostersFromTopToSpecifiedEntity(staticTextId) == new Guid[] { }
                && _.GetRostersFromTopToSpecifiedEntity(interviewerTextQuetionId) == new Guid[] { }
                && _.GetRostersFromTopToSpecifiedEntity(groupId) == new Guid[] { });

            statefulInterview = Create.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            statefulInterview.Apply(Create.Event.RosterInstancesAdded(rosterGroupId: rosterId, rosterInstanceId: rosterInstance2Id, sortIndex:1));
            statefulInterview.Apply(Create.Event.RosterInstancesAdded(rosterGroupId: rosterId, rosterInstanceId: rosterInstance1Id, sortIndex:2));
        };

        Because of = () =>
        {
            identitiesOfEntitiesInSelectedGroup =
                statefulInterview.GetInterviewerEntities(selectedGroupIdentity);

            rosterInstance1Position = GetIndexOfRosterInstancesById(rosterInstance1Id);
            rosterInstance2Position = GetIndexOfRosterInstancesById(rosterInstance2Id);
        };

        It should_contains_5_identities = () =>
            identitiesOfEntitiesInSelectedGroup.Count().ShouldEqual(5);

        It should_contains_group_and_interviewer_text_question_and_static_text = () =>
            identitiesOfEntitiesInSelectedGroup.Select(x=>x.Id).ShouldContain(groupId, interviewerTextQuetionId, staticTextId);

        It should_contains_2_roster_instances = () =>
            identitiesOfEntitiesInSelectedGroup.Count(x => x.Id == rosterId).ShouldEqual(2);

        It should_not_contains_prefilled_text_question = () =>
            identitiesOfEntitiesInSelectedGroup.ShouldNotContain(prefilledTextQuestionId);

        It should_rosterInstance1_has_greater_index_then_rosterInstance2 = () =>
            rosterInstance1Position.ShouldBeGreaterThan(rosterInstance2Position);

        private static int GetIndexOfRosterInstancesById(decimal rosterInstanceId)
        {
            return identitiesOfEntitiesInSelectedGroup.Select((x, i) => new { id = x, index = i })
                    .Where(x => x.id.Equals(new Identity(rosterId, new RosterVector(new[] { rosterInstanceId })))).Select(x => x.index)
                    .FirstOrDefault();
        }

        static StatefulInterview statefulInterview;
        static readonly Identity selectedGroupIdentity = new Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);
        static IEnumerable<Identity> identitiesOfEntitiesInSelectedGroup;
        static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111112");
        static readonly Guid staticTextId = Guid.Parse("11111111111111111111111111111113");
        static readonly Guid prefilledTextQuestionId = Guid.Parse("11111111111111111111111111111114");
        static readonly Guid interviewerTextQuetionId = Guid.Parse("11111111111111111111111111111115");
        static readonly Guid rosterId = Guid.Parse("11111111111111111111111111111116");
        static readonly Guid groupId = Guid.Parse("11111111111111111111111111111117");
        const decimal rosterInstance1Id = 4444m;
        const decimal rosterInstance2Id = 555m;

        private static int rosterInstance1Position;
        private static int rosterInstance2Position;
    }
}
