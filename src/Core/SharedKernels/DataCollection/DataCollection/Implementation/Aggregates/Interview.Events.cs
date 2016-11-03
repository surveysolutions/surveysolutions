using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void ApplyEvents(InterviewTree sourceInterview, InterviewTree changedInterview, Guid? responsibleId = null)
        {
            var diff = sourceInterview.Compare(changedInterview);

            var diffByQuestions = diff.OfType<InterviewTreeQuestionDiff>().ToList();
            var questionsWithRemovedAnswer = diffByQuestions.Where(x => x.IsAnswerRemoved).ToArray();
            var questionsWithChangedAnswer = diffByQuestions.Where(x => x.IsAnswerChanged).ToArray();
            var questionsWithChangedOptionsSet = diffByQuestions.Where(x => x.AreLinkedOptionsChanged).ToArray();
            var changedRosters = diff.OfType<InterviewTreeRosterDiff>().ToArray();
            var changedVariables = diff.OfType<InterviewTreeVariableDiff>().ToArray();

            this.ApplyUpdateAnswerEvents(questionsWithChangedAnswer, responsibleId.Value);
            this.ApplyRemoveAnswerEvents(questionsWithRemovedAnswer);
            this.ApplyRosterEvents(changedRosters);
            this.ApplyEnablementEvents(diff);
            this.ApplyValidityEvents(diff);
            this.ApplyVariableEvents(changedVariables);
            this.ApplyLinkedOptionsChangesEvents(questionsWithChangedOptionsSet);
        }

        private void ApplyLinkedOptionsChangesEvents(InterviewTreeQuestionDiff[] questionsWithChangedOptionsSet)
        {
            var changedLinkedOptions = questionsWithChangedOptionsSet
                .Select(x => new ChangedLinkedOptions(x.ChangedNode.Identity, x.ChangedNode.AsLinked.Options.ToArray()))
                .ToArray();

            if (changedLinkedOptions.Any())
            {
                this.ApplyEvent(new LinkedOptionsChanged(changedLinkedOptions));
            }
        }

        private void ApplyVariableEvents(InterviewTreeVariableDiff[] diffsByChangedVariables)
        {
            var changedVariables = diffsByChangedVariables.Where(x => x.ChangedNode != null && x.IsValueChanged).Select(ToChangedVariable).ToArray();

            if (changedVariables.Any())
                this.ApplyEvent(new VariablesChanged(changedVariables));
        }

        private void ApplyValidityEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff)
        {
            var allNotNullableNodes = diff.Where(x => x.ChangedNode != null).ToList();

            var allChangedQuestionDiffs = allNotNullableNodes.OfType<InterviewTreeQuestionDiff>().ToList();
            var allChangedStaticTextDiffs = allNotNullableNodes.OfType<InterviewTreeStaticTextDiff>().ToList();

            var validQuestionIdentities = allChangedQuestionDiffs.Where(x => x.IsValid).Select(x => x.ChangedNode.Identity).ToArray();
            var invalidQuestionIdentities = allChangedQuestionDiffs.Where(x => x.IsInvalid).Select(x => x.ChangedNode)
                .ToDictionary(x => x.Identity, x => x.FailedValidations);

            var validStaticTextIdentities = allChangedStaticTextDiffs.Where(x => x.IsValid).Select(x => x.ChangedNode.Identity).ToArray();
            var invalidStaticTextIdentities = allChangedStaticTextDiffs.Where(x => x.IsInvalid).Select(x => x.ChangedNode)
                .Select(x => new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(x.Identity, x.FailedValidations))
                .ToList();

            if(validQuestionIdentities.Any()) this.ApplyEvent(new AnswersDeclaredValid(validQuestionIdentities));
            if(invalidQuestionIdentities.Any()) this.ApplyEvent(new AnswersDeclaredInvalid(invalidQuestionIdentities));

            if(validStaticTextIdentities.Any()) this.ApplyEvent(new StaticTextsDeclaredValid(validStaticTextIdentities));
            if(invalidStaticTextIdentities.Any()) this.ApplyEvent(new StaticTextsDeclaredInvalid(invalidStaticTextIdentities));
        }

        private void ApplyEnablementEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff)
        {
            var allNotNullableNodes = diff.Where(x => x.ChangedNode != null).ToList();

            var diffByGroups = allNotNullableNodes.OfType<InterviewTreeGroupDiff>().ToList();
            var diffByQuestions = allNotNullableNodes.OfType<InterviewTreeQuestionDiff>().ToList();
            var diffByStaticTexts = allNotNullableNodes.OfType<InterviewTreeStaticTextDiff>().ToList();
            var diffByVariables = allNotNullableNodes.OfType<InterviewTreeVariableDiff>().ToList();

            var disabledGroups = diffByGroups.Where(x=>x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledGroups = diff.OfType<InterviewTreeGroupDiff>().Where(x => x.IsNodeEnabled).Select(x => (x.ChangedNode ?? x.SourceNode).Identity).ToArray();

            var disabledQuestions = diffByQuestions.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledQuestions = diff.OfType<InterviewTreeQuestionDiff>().Where(x => x.IsNodeEnabled).Select(x => (x.ChangedNode ?? x.SourceNode).Identity).ToArray();

            var disabledStaticTexts = diffByStaticTexts.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledStaticTexts = diffByStaticTexts.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            var disabledVariables = diffByVariables.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledVariables = diffByVariables.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            if(disabledGroups.Any()) this.ApplyEvent(new GroupsDisabled(disabledGroups));
            if (enabledGroups.Any()) this.ApplyEvent(new GroupsEnabled(enabledGroups));
            if (disabledQuestions.Any()) this.ApplyEvent(new QuestionsDisabled(disabledQuestions));
            if(enabledQuestions.Any()) this.ApplyEvent(new QuestionsEnabled(enabledQuestions));
            if(disabledStaticTexts.Any()) this.ApplyEvent(new StaticTextsDisabled(disabledStaticTexts));
            if(enabledStaticTexts.Any()) this.ApplyEvent(new StaticTextsEnabled(enabledStaticTexts));
            if(disabledVariables.Any()) this.ApplyEvent(new VariablesDisabled(disabledVariables));
            if(enabledVariables.Any()) this.ApplyEvent(new VariablesEnabled(enabledVariables));
        }

        private void ApplyUpdateAnswerEvents(InterviewTreeQuestionDiff[] diffByQuestions, Guid responsibleId)
        {
            foreach (var diffByQuestion in diffByQuestions)
            {
                var changedQuestion = diffByQuestion.ChangedNode;

                if (changedQuestion == null) continue;

                if (!changedQuestion.IsAnswered()) continue;

                if (changedQuestion.IsText)
                {
                    this.ApplyEvent(new TextQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsText.GetAnswer().Value));
                }

                if (changedQuestion.IsTextList)
                {
                    this.ApplyEvent(new TextListQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsTextList.GetAnswer().ToTupleArray()));
                }

                if (changedQuestion.IsDouble)
                {
                    this.ApplyEvent(new NumericRealQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, (decimal)changedQuestion.AsDouble.GetAnswer().Value));
                }

                if (changedQuestion.IsInteger)
                {
                    this.ApplyEvent(new NumericIntegerQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsInteger.GetAnswer().Value));
                }

                if (changedQuestion.IsDateTime)
                {
                    this.ApplyEvent(new DateTimeQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsDateTime.GetAnswer().Value));
                }

                if (changedQuestion.IsGps)
                {
                    var gpsAnswer = changedQuestion.AsGps.GetAnswer().Value;
                    this.ApplyEvent(new GeoLocationQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, gpsAnswer.Latitude, gpsAnswer.Longitude,
                        gpsAnswer.Accuracy, gpsAnswer.Altitude, gpsAnswer.Timestamp));
                }

                if (changedQuestion.IsQRBarcode)
                {
                    this.ApplyEvent(new QRBarcodeQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsQRBarcode.GetAnswer().DecodedText));
                }

                if (changedQuestion.IsMultimedia)
                {
                    this.ApplyEvent(new PictureQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsMultimedia.GetAnswer().FileName));
                }

                if (changedQuestion.IsYesNo)
                {
                    this.ApplyEvent(new YesNoQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsYesNo.GetAnswer().ToAnsweredYesNoOptions().ToArray()));
                }

                if (changedQuestion.IsSingleFixedOption)
                {
                    this.ApplyEvent(new SingleOptionQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsSingleFixedOption.GetAnswer().SelectedValue));
                }

                if (changedQuestion.IsMultiFixedOption)
                {
                    this.ApplyEvent(new MultipleOptionsQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsMultiFixedOption.GetAnswer().ToDecimals().ToArray()));
                }

                if (changedQuestion.IsSingleLinkedOption)
                {
                    this.ApplyEvent(new SingleOptionLinkedQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsSingleLinkedOption.GetAnswer().SelectedValue));
                }

                if (changedQuestion.IsMultiLinkedOption)
                {
                    this.ApplyEvent(new MultipleOptionsLinkedQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsMultiLinkedOption.GetAnswer().ToDecimalArrayArray()));
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
            var removedRosters = diff.Where(x => x.IsNodeRemoved).Select(x => x.SourceNode).ToArray();
            var addedRosters = diff.Where(x => x.IsNodeAdded).Select(x => x.ChangedNode).ToArray();
            var changedRosterTitles = diff.Where(x => x.IsRosterTitleChanged).Select(x => x.ChangedNode).ToArray();

            if (removedRosters.Any())
                this.ApplyEvent(new RosterInstancesRemoved(removedRosters.Select(ToRosterInstance).ToArray()));

            if (addedRosters.Any())
                this.ApplyEvent(new RosterInstancesAdded(addedRosters.OrderBy(x => x.RosterVector.Length).Select(ToAddedRosterInstance).ToArray()));

            if (changedRosterTitles.Any())
                this.ApplyEvent(new RosterInstancesTitleChanged(changedRosterTitles.Select(ToChangedRosterInstanceTitleDto).ToArray()));
        }

        private void ApplySubstitutionEvents(InterviewTree tree, IQuestionnaire questionnaire, List<Identity> changedQuestionIdentities)
        {
            var changedQuestionTitles = new List<Identity>();
            var changedStaticTextTitles = new List<Identity>();
            var changedGroupTitles = new List<Identity>();
            foreach (var questionIdentity in changedQuestionIdentities)
            {
                var rosterLevel = questionIdentity.RosterVector.Length;

                var substitutedQuestionIds = questionnaire.GetSubstitutedQuestions(questionIdentity.Id);
                foreach (var substitutedQuestionId in substitutedQuestionIds)
                {
                    changedQuestionTitles.AddRange(tree.FindEntity(substitutedQuestionId)
                        .Select(x => x.Identity)
                        .Where(x => x.RosterVector.Take(rosterLevel).SequenceEqual(questionIdentity.RosterVector)));
                }

                var substitutedStaticTextIds = questionnaire.GetSubstitutedStaticTexts(questionIdentity.Id);
                foreach (var substitutedStaticTextId in substitutedStaticTextIds)
                {
                    changedStaticTextTitles.AddRange(tree.FindEntity(substitutedStaticTextId)
                        .Select(x => x.Identity)
                        .Where(x => x.RosterVector.Take(rosterLevel).SequenceEqual(questionIdentity.RosterVector)));
                }

                var substitutedGroupIds = questionnaire.GetSubstitutedGroups(questionIdentity.Id);
                foreach (var substitutedGroupId in substitutedGroupIds)
                {
                    changedGroupTitles.AddRange(tree.FindEntity(substitutedGroupId)
                        .Select(x => x.Identity)
                        .Where(x => x.RosterVector.Take(rosterLevel).SequenceEqual(questionIdentity.RosterVector)));
                }
            }

            if (changedQuestionTitles.Any() || changedStaticTextTitles.Any() || changedGroupTitles.Any())
            {
                this.ApplyEvent(new SubstitutionTitlesChanged(
                    changedQuestionTitles.ToArray(),
                    changedStaticTextTitles.ToArray(),
                    changedGroupTitles.ToArray()));
            }
        }
        
        private static ChangedRosterInstanceTitleDto ToChangedRosterInstanceTitleDto(InterviewTreeRoster roster)
            => new ChangedRosterInstanceTitleDto(ToRosterInstance(roster), roster.RosterTitle);

        private static AddedRosterInstance ToAddedRosterInstance(IInterviewTreeNode rosterNode)
            => new AddedRosterInstance(rosterNode.Identity.Id, rosterNode.Identity.RosterVector.Shrink(), rosterNode.Identity.RosterVector.Last(), (rosterNode as InterviewTreeRoster).SortIndex);

        private static RosterInstance ToRosterInstance(IInterviewTreeNode rosterNode)
            => new RosterInstance(rosterNode.Identity.Id, rosterNode.Identity.RosterVector.Shrink(), rosterNode.Identity.RosterVector.Last());

        private static ChangedVariable ToChangedVariable(InterviewTreeVariableDiff variable)
            => new ChangedVariable(variable.ChangedNode.Identity, variable.ChangedNode.Value);
    }
}