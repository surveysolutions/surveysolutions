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
        public void ApplyEvents(InterviewTree sourceInterview, InterviewTree changedInterview, Guid responsibleId)
        {
            var diff = sourceInterview.Compare(changedInterview);

            var questionsWithRemovedAnswer = diff.OfType<InterviewTreeQuestionDiff>().Where(IsAnswerRemoved).ToArray();
            var questionsWithChangedAnswer = diff.OfType<InterviewTreeQuestionDiff>().Except(questionsWithRemovedAnswer).ToArray();
            var changedRosters = diff.OfType<InterviewTreeRosterDiff>().ToArray();

            this.ApplyUpdateAnswerEvents(questionsWithChangedAnswer, responsibleId);
            this.ApplyRemoveAnswerEvents(questionsWithRemovedAnswer);
            this.ApplyRosterEvents(changedRosters);
            this.ApplyEnablementEvents(diff);
        }

        private void ApplyEnablementEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff)
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

        private void ApplyUpdateAnswerEvents(InterviewTreeQuestionDiff[] diffByQuestions, Guid responsibleId)
        {
            foreach (var diffByQuestion in diffByQuestions)
            {
                var changedQuestion = diffByQuestion.ChangedNode;

                if (changedQuestion == null) continue;

                if (changedQuestion.IsText)
                {
                    this.ApplyEvent(new TextQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsText.GetAnswer()));
                }

                if (changedQuestion.IsTextList)
                {
                    this.ApplyEvent(new TextListQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsTextList.GetAnswer()));
                }

                if (changedQuestion.IsDouble)
                {
                    this.ApplyEvent(new NumericRealQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, (decimal)changedQuestion.AsDouble.GetAnswer()));
                }

                if (changedQuestion.IsInteger)
                {
                    this.ApplyEvent(new NumericIntegerQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsInteger.GetAnswer()));
                }

                if (changedQuestion.IsDateTime)
                {
                    this.ApplyEvent(new DateTimeQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsDateTime.GetAnswer()));
                }

                if (changedQuestion.IsGps)
                {
                    var gpsAnswer = changedQuestion.AsGps.GetAnswer();
                    this.ApplyEvent(new GeoLocationQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, gpsAnswer.Latitude, gpsAnswer.Longitude,
                        gpsAnswer.Accuracy, gpsAnswer.Altitude, gpsAnswer.Timestamp));
                }

                if (changedQuestion.IsQRBarcode)
                {
                    this.ApplyEvent(new QRBarcodeQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsQRBarcode.GetAnswer()));
                }

                if (changedQuestion.IsMultimedia)
                {
                    this.ApplyEvent(new PictureQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsMultimedia.GetAnswer()));
                }

                if (changedQuestion.IsYesNo)
                {
                    this.ApplyEvent(new YesNoQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsYesNo.GetAnswer()));
                }

                if (changedQuestion.IsSingleOption)
                {
                    this.ApplyEvent(new SingleOptionQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsSingleOption.GetAnswer()));
                }

                if (changedQuestion.IsMultiOption)
                {
                    this.ApplyEvent(new MultipleOptionsQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsMultiOption.GetAnswer()));
                }

                if (changedQuestion.IsSingleLinkedOption)
                {
                    this.ApplyEvent(new SingleOptionLinkedQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsSingleLinkedOption.GetAnswer()));
                }

                if (changedQuestion.IsMultiLinkedOption)
                {
                    this.ApplyEvent(new MultipleOptionsLinkedQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsMultiLinkedOption.GetAnswer()));
                }
            }
        }

        private void ApplyRemoveAnswerEvents(InterviewTreeQuestionDiff[] diffByQuestions)
        {
            var questionIdentittiesWithRemovedAnswer = diffByQuestions.Select(x => x.SourceNode.Identity).ToArray();

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