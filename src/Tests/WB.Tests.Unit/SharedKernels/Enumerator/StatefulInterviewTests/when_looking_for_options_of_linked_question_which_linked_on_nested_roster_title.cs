using System;
using System.Linq;
using Machine.Specifications;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_looking_for_options_of_linked_question_which_linked_on_nested_roster_title : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            linkedQuestionIdentity = Create.Entity.Identity(linkedQuestionId, RosterVector.Empty);
            newOptionsEvent = new[] {
                new ChangedLinkedOptions(linkedQuestionIdentity,
                                         new []
                                         {
                                             Create.Entity.RosterVector(1,1),
                                             Create.Entity.RosterVector(2,1),
                                             Create.Entity.RosterVector(1,2),
                                             Create.Entity.RosterVector(2,2)
                                         })
            };
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            nestedRosterId = Guid.NewGuid();
            IQuestionnaire questionnaire = Substitute.For<IQuestionnaire>();
            questionnaire.GetRosterReferencedByLinkedQuestion(linkedQuestionId)
                .Returns(nestedRosterId);
            questionnaire.GetRostersFromTopToSpecifiedGroup(nestedRosterId)
                .Returns(new[] { rosterId, nestedRosterId });

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.Apply(Create.Event.RosterInstancesAdded(rosterId, new decimal[0], 1, 1));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterId, new decimal[0], 2, 2));

            interview.Apply(Create.Event.RosterInstancesAdded(nestedRosterId, new decimal[] { 1 }, 1, 1));
            interview.Apply(Create.Event.RosterInstancesAdded(nestedRosterId, new decimal[] { 1 }, 2, 2));
            interview.Apply(Create.Event.RosterInstancesAdded(nestedRosterId, new decimal[] { 2 }, 1, 1));
            interview.Apply(Create.Event.RosterInstancesAdded(nestedRosterId, new decimal[] { 2 }, 2, null));
            
            interview.Apply(Create.Event.RosterInstancesTitleChanged(nestedRosterId, Create.Entity.RosterVector(1, 1), "a"));
            interview.Apply(Create.Event.RosterInstancesTitleChanged(nestedRosterId, Create.Entity.RosterVector(1, 2), "b"));
            interview.Apply(Create.Event.RosterInstancesTitleChanged(nestedRosterId, Create.Entity.RosterVector(2, 1), "a"));
            interview.Apply(Create.Event.RosterInstancesTitleChanged(nestedRosterId, Create.Entity.RosterVector(2, 2), "b"));
            interview.Apply(Create.Event.LinkedOptionsChanged(newOptionsEvent));
        };

        Because of = () => rosters = interview.FindReferencedRostersForLinkedQuestion(nestedRosterId, linkedQuestionIdentity).ToArray();

        It should_order_options_by_roster_sort_index_at_first = () =>
        {
            rosters.Select(a => a.RosterVector.First()).ToArray().ShouldEqual(new decimal[] { 1, 1, 2, 2 });
        };

        It should_order_options_by_nested_roster_sort_index_in_scope_of_parent_roster = () =>
        {
            rosters.Select(a => a.RosterVector.Last()).ToArray().ShouldEqual(new decimal[] { 1, 2, 1, 2 });
        };

        static StatefulInterview interview;
        static Guid linkedQuestionId;
        static Guid nestedRosterId;
        static ChangedLinkedOptions[] newOptionsEvent;
        static Identity linkedQuestionIdentity;
        static InterviewRoster[] rosters;
    }
}