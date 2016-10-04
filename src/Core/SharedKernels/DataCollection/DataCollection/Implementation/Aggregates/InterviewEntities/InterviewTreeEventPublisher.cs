using System;
using System.Linq;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeEventPublisher : EventSourcedAggregateRoot
    {
        public InterviewTreeEventPublisher(Guid interviewId) : base(interviewId) { }

        public void ApplyRosterEvents(InterviewTree sourceInterview, InterviewTree changedInterview, IQuestionnaire questionnaire)
        {
            var diff = sourceInterview.FindDiff(changedInterview);

            var removedRosters = diff
                .Where(x => x.SourceNode is InterviewTreeRoster && x.ChangedNode == null)
                .Select(x => x.SourceNode)
                .ToArray();

            var addedRosters = diff
                .Where(x => x.ChangedNode is InterviewTreeRoster && x.SourceNode == null)
                .Select(x => x.ChangedNode)
                .ToArray();

            var removedAnswersByRemovedRosters = removedRosters
                .TreeToEnumerable(x => x.Children)
                .OfType<InterviewTreeQuestion>()
                .Select(x => x.Identity)
                .ToArray();

            var rosterTitleQuestionIds = questionnaire.GetRostersWithTitlesToChange().ToArray();

            var changedRosterTitleQuestions = diff
                .Where(x => IsChangedRosterTitleQuestion(x, rosterTitleQuestionIds))
                .Select(x => x.ChangedNode)
                .Cast<InterviewTreeQuestion>()
                .ToArray();


            if (removedRosters.Any())
                this.ApplyEvent(new RosterInstancesRemoved(removedRosters.Select(ToRosterInstance).ToArray()));

            if (removedAnswersByRemovedRosters.Any())
                this.ApplyEvent(new AnswersRemoved(removedAnswersByRemovedRosters));

            if (addedRosters.Any())
                this.ApplyEvent(new RosterInstancesAdded(addedRosters.Select(ToAddedRosterInstance).ToArray()));

            if(changedRosterTitleQuestions.Any())
                this.ApplyEvent(new RosterInstancesTitleChanged(changedRosterTitleQuestions.Select(ToChangedRosterInstanceTitleDto).ToArray()));
        }

        private ChangedRosterInstanceTitleDto ToChangedRosterInstanceTitleDto(InterviewTreeQuestion rosterTitleQuestion)
            => new ChangedRosterInstanceTitleDto(
                new RosterInstance(rosterTitleQuestion.Parent.Identity.Id,
                    rosterTitleQuestion.Parent.Identity.RosterVector,
                    rosterTitleQuestion.Identity.RosterVector.Last()), rosterTitleQuestion.Title);

        private bool IsChangedRosterTitleQuestion(InterviewTreeNodeDiff diff, Guid[] rosterTitleQuestionIds)
        {
            var sourceQuestion = diff.SourceNode as InterviewTreeQuestion;
            var changedQuestion = diff.ChangedNode as InterviewTreeQuestion;

            if (sourceQuestion == null || changedQuestion == null) return false;
            if (!rosterTitleQuestionIds.Contains(sourceQuestion.Identity.Id)) return false;

            return !sourceQuestion.Equals(changedQuestion.Answer);
        }

        private static AddedRosterInstance ToAddedRosterInstance(IInterviewTreeNode rosterNode)
            => new AddedRosterInstance(rosterNode.Identity.Id, rosterNode.Parent.Identity.RosterVector, rosterNode.Identity.RosterVector.Last(), 0);

        private static RosterInstance ToRosterInstance(IInterviewTreeNode x)
            => new RosterInstance(x.Identity.Id, x.Parent.Identity.RosterVector, x.Identity.RosterVector.Last());
    }
}