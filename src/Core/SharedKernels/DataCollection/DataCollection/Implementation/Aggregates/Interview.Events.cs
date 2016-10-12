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

            this.ApplyRosterEvents(diff.OfType<InterviewTreeRosterDiff>().ToArray());
            this.ApplyRemoveAnswerEvents(diff.OfType<InterviewTreeQuestionDiff>().ToArray());
            this.ApplyDisableEvents(diff);
        }

        private void ApplyDisableEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff)
        {
            var allNotNullableNodes = diff.Where(x => x.SourceNode != null && x.ChangedNode != null).ToList();

            var disabledGroups = allNotNullableNodes.OfType<InterviewTreeGroupDiff>().Where(IsDisabledNode).Select(x => x.SourceNode.Identity).ToArray();
            var enabledGroups = allNotNullableNodes.OfType<InterviewTreeGroupDiff>().Where(IsEnabledNode).Select(x => x.SourceNode.Identity).ToArray();

            var disabledQuestions = allNotNullableNodes.OfType<InterviewTreeQuestionDiff>().Where(IsDisabledNode).Select(x => x.SourceNode.Identity).ToArray();
            var enabledQuestions = allNotNullableNodes.OfType<InterviewTreeQuestionDiff>().Where(IsEnabledNode).Select(x => x.SourceNode.Identity).ToArray();

            var disabledStaticTexts = allNotNullableNodes.OfType<InterviewTreeStaticTextDiff>().Where(IsDisabledNode).Select(x => x.SourceNode.Identity).ToArray();
            var enabledStaticTexts = allNotNullableNodes.OfType<InterviewTreeStaticTextDiff>().Where(IsEnabledNode).Select(x => x.SourceNode.Identity).ToArray();

            var disabledVariables = allNotNullableNodes.OfType<InterviewTreeVariableDiff>().Where(IsDisabledNode).Select(x => x.SourceNode.Identity).ToArray();
            var enabledVariables = allNotNullableNodes.OfType<InterviewTreeVariableDiff>().Where(IsEnabledNode).Select(x => x.SourceNode.Identity).ToArray();

            if(disabledGroups.Any()) this.Apply(new GroupsDisabled(disabledGroups));
            if (enabledGroups.Any()) this.Apply(new GroupsEnabled(enabledGroups));
            if (disabledQuestions.Any()) this.Apply(new QuestionsDisabled(disabledQuestions));
            if(enabledQuestions.Any()) this.Apply(new QuestionsEnabled(enabledQuestions));
            if(disabledStaticTexts.Any()) this.Apply(new StaticTextsDisabled(disabledStaticTexts));
            if(enabledStaticTexts.Any()) this.Apply(new StaticTextsEnabled(enabledStaticTexts));
            if(disabledVariables.Any()) this.Apply(new VariablesDisabled(disabledVariables));
            if(enabledVariables.Any()) this.Apply(new VariablesEnabled(enabledVariables));
        }

        private void ApplyRemoveAnswerEvents(InterviewTreeQuestionDiff[] diff)
        {
            var questionIdentittiesWithRemovedAnswer = diff
                .Where(IsAnswerRemoved)
                .Select(x => x.SourceNode.Identity)
                .ToArray();

            if (questionIdentittiesWithRemovedAnswer.Any())
                this.ApplyEvent(new AnswersRemoved(questionIdentittiesWithRemovedAnswer));
        }

        private void ApplyRosterEvents(InterviewTreeRosterDiff[] diff)
        {
            var removedRosters = diff
                .Where(x => x.SourceNode != null && x.ChangedNode == null)
                .Select(x => x.SourceNode)
                .ToArray();

            var addedRosters = diff
                .Where(x => x.SourceNode == null && x.ChangedNode != null)
                .Select(x => x.ChangedNode)
                .ToArray();

            var changedRosterTitles = diff
                .Where(x => x.ChangedNode != null && x.SourceNode?.RosterTitle != x.ChangedNode.RosterTitle)
                .Select(x => x.ChangedNode)
                .ToArray();

            if (removedRosters.Any())
                this.ApplyEvent(new RosterInstancesRemoved(removedRosters.Select(ToRosterInstance).ToArray()));

            if (addedRosters.Any())
                this.ApplyEvent(new RosterInstancesAdded(addedRosters.Select(ToAddedRosterInstance).ToArray()));

            if (changedRosterTitles.Any())
                this.ApplyEvent(new RosterInstancesTitleChanged(changedRosterTitles.Select(ToChangedRosterInstanceTitleDto).ToArray()));
        }

        private static bool IsDisabledNode(InterviewTreeNodeDiff diff)
            => !diff.SourceNode.IsDisabled() && diff.ChangedNode.IsDisabled();

        private static bool IsEnabledNode(InterviewTreeNodeDiff diff)
            => diff.SourceNode.IsDisabled() && !diff.ChangedNode.IsDisabled();

        private static bool IsAnswerRemoved(InterviewTreeQuestionDiff diff) => diff.SourceNode != null &&
                                                                               diff.SourceNode.IsAnswered() && (diff.ChangedNode == null || !diff.ChangedNode.IsAnswered());

        private static ChangedRosterInstanceTitleDto ToChangedRosterInstanceTitleDto(InterviewTreeRoster roster)
            => new ChangedRosterInstanceTitleDto(ToRosterInstance(roster), roster.RosterTitle);

        private static AddedRosterInstance ToAddedRosterInstance(IInterviewTreeNode rosterNode)
            => new AddedRosterInstance(rosterNode.Identity.Id, rosterNode.Identity.RosterVector.Shrink(), rosterNode.Identity.RosterVector.Last(), 0);

        private static RosterInstance ToRosterInstance(IInterviewTreeNode rosterNode)
            => new RosterInstance(rosterNode.Identity.Id, rosterNode.Identity.RosterVector.Shrink(), rosterNode.Identity.RosterVector.Last());
    }
}