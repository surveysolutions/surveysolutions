using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void ApplyEvents(InterviewTree sourceInterview, InterviewTree changedInterview)
        {
            var diff = sourceInterview.Compare(changedInterview);

            this.ApplyRosterEvents(diff);
            this.ApplyRemoveAnswerEvents(diff);
        }

        private void ApplyRemoveAnswerEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff)
        {
            var questionIdentittiesWithRemovedAnswer = diff
                .Where(IsAnswerRemoved)
                .Select(x => x.SourceNode.Identity)
                .ToArray();

            if (questionIdentittiesWithRemovedAnswer.Any())
                this.ApplyEvent(new AnswersRemoved(questionIdentittiesWithRemovedAnswer));
        }

        private void ApplyRosterEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff)
        {
            var removedRosters = diff
                .Where(x => x.SourceNode is InterviewTreeRoster && x.ChangedNode == null)
                .Select(x => x.SourceNode)
                .ToArray();

            var addedRosters = diff
                .Where(x => x.SourceNode == null && x.ChangedNode is InterviewTreeRoster)
                .Select(x => x.ChangedNode)
                .ToArray();

            var changedRosterTitles = diff
                .Where(HasChangedRosterTitle)
                .Select(x => x.ChangedNode)
                .Cast<InterviewTreeRoster>()
                .ToArray();

            if (removedRosters.Any())
                this.ApplyEvent(new RosterInstancesRemoved(removedRosters.Select(ToRosterInstance).ToArray()));

            if (addedRosters.Any())
                this.ApplyEvent(new RosterInstancesAdded(addedRosters.Select(ToAddedRosterInstance).ToArray()));

            if (changedRosterTitles.Any())
                this.ApplyEvent(new RosterInstancesTitleChanged(changedRosterTitles.Select(ToChangedRosterInstanceTitleDto).ToArray()));
        }

        private static bool HasChangedRosterTitle(InterviewTreeNodeDiff diffNode)
        {
            var nodeWasDeletedOrIsNotRosterNode = !(diffNode.ChangedNode is InterviewTreeRoster);
            if (nodeWasDeletedOrIsNotRosterNode)
                return false;

            var nodeIsNewlyAddedRoster = diffNode.SourceNode == null;
            if (nodeIsNewlyAddedRoster)
                return true;

            var sourceRoster = diffNode.SourceNode as InterviewTreeRoster;

            if (sourceRoster == null)
                throw new Exception("Diff works wrong! Roster cannot be compared with non-roster element");

            var changedRoster = (InterviewTreeRoster) diffNode.ChangedNode;

            return sourceRoster.RosterTitle != changedRoster.RosterTitle;
        }

        private static bool IsAnswerRemoved(InterviewTreeNodeDiff diff)
        {
            var sourceQuestion = diff.SourceNode as InterviewTreeQuestion;
            var changedQuestion = diff.ChangedNode as InterviewTreeQuestion;

            if (sourceQuestion == null) return false;

            return sourceQuestion.IsAnswered() && (changedQuestion == null || !changedQuestion.IsAnswered());
        }

        private static ChangedRosterInstanceTitleDto ToChangedRosterInstanceTitleDto(InterviewTreeRoster roster)
            => new ChangedRosterInstanceTitleDto(ToRosterInstance(roster), roster.RosterTitle);

        private static AddedRosterInstance ToAddedRosterInstance(IInterviewTreeNode rosterNode)
            => new AddedRosterInstance(rosterNode.Identity.Id, rosterNode.Identity.RosterVector.Shrink(), rosterNode.Identity.RosterVector.Last(), 0);

        private static RosterInstance ToRosterInstance(IInterviewTreeNode rosterNode)
            => new RosterInstance(rosterNode.Identity.Id, rosterNode.Identity.RosterVector.Shrink(), rosterNode.Identity.RosterVector.Last());
    }
}