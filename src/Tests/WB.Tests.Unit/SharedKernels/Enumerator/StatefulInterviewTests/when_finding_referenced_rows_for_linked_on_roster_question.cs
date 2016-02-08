using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_finding_referenced_rows_for_linked_on_roster_question : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionRosterVector = new decimal[] { };
            var linkedQuestionRosters = new Guid[] { };

            var referencedQuestionRosters = new[] { rosterId };

            IPlainQuestionnaireRepository questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.HasQuestion(linkedQuestionId) == true
                && _.GetRosterLevelForQuestion(linkedQuestionId) == linkedQuestionRosters.Length
                && _.GetRostersFromTopToSpecifiedQuestion(linkedQuestionId) == linkedQuestionRosters
                && _.GetRosterSizeSourcesForEntity(linkedQuestionId) == linkedQuestionRosters

                && _.GetRosterLevelForGroup(rosterId) == referencedQuestionRosters.Length
                && _.GetRostersFromTopToSpecifiedGroup(rosterId)== referencedQuestionRosters
                && _.GetRosterSizeSourcesForEntity(rosterId) == referencedQuestionRosters);

            interview = Create.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.Apply(Create.Event.RosterInstancesAdded(rosterId,
               new[] { 1m },
               new[] { 2m },
               new[] { 3m }));

            interview.Apply(Create.Event.RosterInstancesTitleChanged(rosterId, new[] { 1m },"1-1"));
            interview.Apply(Create.Event.RosterInstancesTitleChanged(rosterId, new[] { 2m }, "2-2"));
            interview.Apply(Create.Event.GroupsDisabled(rosterId, new[] {2m}));
        };

        Because of = () =>
            result = interview.FindReferencedRostersForLinkedQuestion(rosterId, Create.Identity(linkedQuestionId, linkedQuestionRosterVector));

        It should_return_only_enabled_row_with_not_empty_title = () =>
            result.Select(answer => answer.Title)
                .ShouldContainOnly("1-1");

        private static StatefulInterview interview;
        private static IEnumerable<InterviewRoster> result;
        private static Guid rosterId = Guid.Parse("55555555555555555555555555555555");
        private static Guid linkedQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] linkedQuestionRosterVector;
        private static Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}