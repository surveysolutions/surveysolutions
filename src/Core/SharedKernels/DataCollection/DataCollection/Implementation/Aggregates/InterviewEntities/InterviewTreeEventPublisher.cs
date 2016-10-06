using System;
using System.Linq;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public static class InterviewTreeEventPublisher
    {
        public static void ApplyRosterEvents(Action<IEvent> ApplyEvent, InterviewTree sourceInterview, InterviewTree changedInterview)
        {
            var diff = sourceInterview.FindDiff(changedInterview);

            var removedRosters = diff
                .Where(x => x.SourceNode is InterviewTreeRoster && x.ChangedNode == null)
                .Select(x => x.SourceNode)
                .ToArray();

            var addedRosters = diff
                .Where(x => x.SourceNode == null && x.ChangedNode is InterviewTreeRoster)
                .Select(x => x.ChangedNode)
                .ToArray();

            var removedAnswersByRemovedRosters = removedRosters
                .TreeToEnumerable(x => x.Children)
                .DistinctBy(x => x.Identity)
                .OfType<InterviewTreeQuestion>()
                .Select(x => x.Identity)
                .ToArray();

            var changedRosterTitles = diff
                .Where(HasChangedRosterTitle)
                .Select(x => x.ChangedNode)
                .Cast<InterviewTreeRoster>()
                .ToArray();


            if (removedRosters.Any())
                ApplyEvent(new RosterInstancesRemoved(removedRosters.Select(ToRosterInstance).ToArray()));

            if (removedAnswersByRemovedRosters.Any())
                ApplyEvent(new AnswersRemoved(removedAnswersByRemovedRosters));

            if (addedRosters.Any())
                ApplyEvent(new RosterInstancesAdded(addedRosters.Select(ToAddedRosterInstance).ToArray()));

            if(changedRosterTitles.Any())
                ApplyEvent(new RosterInstancesTitleChanged(changedRosterTitles.Select(ToChangedRosterInstanceTitleDto).ToArray()));
        }

        private static bool HasChangedRosterTitle(InterviewTreeNodeDiff diffNode)
        {
            var sourceRoster = diffNode.SourceNode as InterviewTreeRoster;
            var changedRoster = diffNode.ChangedNode as InterviewTreeRoster;

            if (sourceRoster == null || changedRoster == null) return false;

            return sourceRoster.RosterTitle != changedRoster.RosterTitle;
        }

        private static ChangedRosterInstanceTitleDto ToChangedRosterInstanceTitleDto(InterviewTreeRoster roster)
            => new ChangedRosterInstanceTitleDto(ToRosterInstance(roster), roster.RosterTitle);

        private static AddedRosterInstance ToAddedRosterInstance(IInterviewTreeNode rosterNode)
            => new AddedRosterInstance(rosterNode.Identity.Id, rosterNode.Parent.Identity.RosterVector, rosterNode.Identity.RosterVector.Last(), 0);

        private static RosterInstance ToRosterInstance(IInterviewTreeNode rosterNode)
            => new RosterInstance(rosterNode.Identity.Id, rosterNode.Parent.Identity.RosterVector, rosterNode.Identity.RosterVector.Last());
    }
}