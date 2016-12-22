using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        protected void UpdateExpressionState(InterviewTree changedInterview, ILatestInterviewExpressionState expressionState)
            => this.UpdateExpressionState(this.Tree, changedInterview, expressionState);

        protected void UpdateExpressionState(InterviewTree sourceInterview, InterviewTree changedInterview, ILatestInterviewExpressionState expressionState)
        {
            var diff = sourceInterview.Compare(changedInterview);

            var diffByQuestions = diff.OfType<InterviewTreeQuestionDiff>().ToList();
            var questionsWithRemovedAnswer = diffByQuestions.Where(x => x.IsAnswerRemoved).ToArray();
            var questionsWithChangedAnswer = diffByQuestions.Where(x => x.IsAnswerChanged).ToArray();
            var changedRosters = diff.OfType<InterviewTreeRosterDiff>().ToArray();
            var changedVariables = diff.OfType<InterviewTreeVariableDiff>().ToArray();

            UpdateRostersInExpressionState(changedRosters, expressionState);
            UpdateAnswersInExpressionState(questionsWithChangedAnswer, expressionState);
            RemoveAnswersInExpressionState(questionsWithRemovedAnswer, expressionState);
            UpdateEnablementInExpressionState(diff, expressionState);
            UpdateValidityInExpressionState(diff, expressionState);
            UpdateVariablesInExpressionState(changedVariables, expressionState);
        }

        private void UpdateVariablesInExpressionState(InterviewTreeVariableDiff[] diffsByChangedVariables, ILatestInterviewExpressionState expressionState)
        {
            var changedVariables = diffsByChangedVariables.Where(x => x.ChangedNode != null && x.IsValueChanged)
                    .Select(x => x.ChangedNode)
                    .ToArray();

            foreach (var changedVariable in changedVariables)
            {
                expressionState.UpdateVariableValue(changedVariable.Identity, changedVariable.Value);
            }
        }

        private static void UpdateValidityInExpressionState(IReadOnlyCollection<InterviewTreeNodeDiff> diff, ILatestInterviewExpressionState expressionState)
        {
            var allNotNullableNodes = diff.Where(x => x.ChangedNode != null).ToList();

            var allChangedQuestionDiffs = allNotNullableNodes.OfType<InterviewTreeQuestionDiff>().ToList();
            var allChangedStaticTextDiffs = allNotNullableNodes.OfType<InterviewTreeStaticTextDiff>().ToList();

            var validQuestionIdentities = allChangedQuestionDiffs.Where(x => x.IsValid).Select(x => x.ChangedNode.Identity).ToArray();
            var invalidQuestionIdentities = allChangedQuestionDiffs.Where(x => x.IsInvalid).Select(x => x.ChangedNode)
                .ToDictionary(x => x.Identity, x => x.FailedValidations);

            var validStaticTextIdentities = allChangedStaticTextDiffs.Where(x => x.IsValid).Select(x => x.ChangedNode.Identity).ToArray();
            var invalidStaticTextIdentities = allChangedStaticTextDiffs.Where(x => x.IsInvalid).Select(x => x.ChangedNode)
                .ToDictionary(x => x.Identity, x => x.FailedValidations);

            if (validQuestionIdentities.Any()) expressionState.DeclareAnswersValid(validQuestionIdentities);
            if (invalidQuestionIdentities.Any()) expressionState.ApplyFailedValidations(invalidQuestionIdentities);

            if (validStaticTextIdentities.Any()) expressionState.DeclareStaticTextValid(validStaticTextIdentities);
            if (invalidStaticTextIdentities.Any()) expressionState.ApplyStaticTextFailedValidations(invalidStaticTextIdentities);
        }

        private static void UpdateEnablementInExpressionState(IReadOnlyCollection<InterviewTreeNodeDiff> diff, ILatestInterviewExpressionState expressionState)
        {
            var allNotNullableNodes = diff.Where(x => x.ChangedNode != null).ToList();

            var diffByGroups = allNotNullableNodes.OfType<InterviewTreeGroupDiff>().ToList();
            var diffByQuestions = allNotNullableNodes.OfType<InterviewTreeQuestionDiff>().ToList();
            var diffByStaticTexts = allNotNullableNodes.OfType<InterviewTreeStaticTextDiff>().ToList();
            var diffByVariables = allNotNullableNodes.OfType<InterviewTreeVariableDiff>().ToList();

            var disabledGroups = diffByGroups.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledGroups = diffByGroups.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            var disabledQuestions = diffByQuestions.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledQuestions = diffByQuestions.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            var disabledStaticTexts = diffByStaticTexts.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledStaticTexts = diffByStaticTexts.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            var disabledVariables = diffByVariables.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledVariables = diffByVariables.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            if (disabledGroups.Any()) expressionState.DisableGroups(disabledGroups);
            if (enabledGroups.Any()) expressionState.EnableGroups(enabledGroups);
            if (disabledQuestions.Any()) expressionState.DisableQuestions(disabledQuestions);
            if (enabledQuestions.Any()) expressionState.EnableQuestions(enabledQuestions);
            if (disabledStaticTexts.Any()) expressionState.DisableStaticTexts(disabledStaticTexts);
            if (enabledStaticTexts.Any()) expressionState.EnableStaticTexts(enabledStaticTexts);
            if (disabledVariables.Any()) expressionState.DisableVariables(disabledVariables);
            if (enabledVariables.Any()) expressionState.EnableVariables(enabledVariables);
        }

        private static void RemoveAnswersInExpressionState(IReadOnlyCollection<InterviewTreeNodeDiff> diffByQuestions, ILatestInterviewExpressionState expressionState)
        {
            foreach (var diffByQuestion in diffByQuestions)
            {
                expressionState.RemoveAnswer(diffByQuestion.SourceNode.Identity);
            }
        }

        private static void UpdateAnswersInExpressionState(InterviewTreeQuestionDiff[] diffByQuestions, ILatestInterviewExpressionState expressionState)
        {
            foreach (var diffByQuestion in diffByQuestions)
            {
                var changedQuestion = diffByQuestion.ChangedNode;

                if (changedQuestion == null) continue;

                if (!changedQuestion.IsAnswered()) continue;

                if (changedQuestion.IsText)
                {
                    expressionState.UpdateTextAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsText.GetAnswer().Value);
                }

                if (changedQuestion.IsTextList)
                {
                    expressionState.UpdateTextListAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsTextList.GetAnswer().ToTupleArray());
                }

                if (changedQuestion.IsDouble)
                {
                    expressionState.UpdateNumericRealAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsDouble.GetAnswer().Value);
                }

                if (changedQuestion.IsInteger)
                {
                    expressionState.UpdateNumericIntegerAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsInteger.GetAnswer().Value);
                }

                if (changedQuestion.IsDateTime)
                {
                    expressionState.UpdateDateAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsDateTime.GetAnswer().Value);
                }

                if (changedQuestion.IsGps)
                {
                    var gpsAnswer = changedQuestion.AsGps.GetAnswer().Value;
                    expressionState.UpdateGeoLocationAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, gpsAnswer.Latitude, gpsAnswer.Longitude,
                        gpsAnswer.Accuracy, gpsAnswer.Altitude);
                }

                if (changedQuestion.IsQRBarcode)
                {
                    expressionState.UpdateQrBarcodeAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsQRBarcode.GetAnswer().DecodedText);
                }

                if (changedQuestion.IsMultimedia)
                {
                    expressionState.UpdateMediaAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsMultimedia.GetAnswer().FileName);
                }

                if (changedQuestion.IsYesNo)
                {
                    expressionState.UpdateYesNoAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsYesNo.GetAnswer().ToYesNoAnswersOnly());
                }

                if (changedQuestion.IsSingleFixedOption)
                {
                    expressionState.UpdateSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsSingleFixedOption.GetAnswer().SelectedValue);
                }

                if (changedQuestion.IsMultiFixedOption)
                {
                    expressionState.UpdateMultiOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsMultiFixedOption.GetAnswer().ToDecimals().ToArray());
                }

                if (changedQuestion.IsSingleLinkedOption)
                {
                    expressionState.UpdateLinkedSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsSingleLinkedOption.GetAnswer().SelectedValue);
                }

                if (changedQuestion.IsMultiLinkedOption)
                {
                    expressionState.UpdateLinkedMultiOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsMultiLinkedOption.GetAnswer().ToDecimalArrayArray());
                }

                if (changedQuestion.IsSingleLinkedToList)
                {
                    expressionState.UpdateSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsSingleLinkedToList.GetAnswer().SelectedValue);
                }

                if (changedQuestion.IsMultiLinkedToList)
                {
                    expressionState.UpdateMultiOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsMultiLinkedToList.GetAnswer().ToDecimals().ToArray());
                }
            }
        }

        private static void UpdateRostersInExpressionState(InterviewTreeRosterDiff[] diff, ILatestInterviewExpressionState expressionState)
        {
            var removedRosters = diff.Where(x => x.IsNodeRemoved).Select(x => x.SourceNode).ToArray();
            var addedRosters = diff.Where(x => x.IsNodeAdded)
                .Select(x => x.ChangedNode)
                .OrderBy(x => x.RosterVector.Length)
                .ToArray();

            foreach (var removedRosterIdentity in removedRosters.Select(ToRosterInstance))
            {
                expressionState.RemoveRoster(removedRosterIdentity.GroupId, removedRosterIdentity.OuterRosterVector, removedRosterIdentity.RosterInstanceId);
            }

            foreach (var rosterNode in addedRosters)
            {
                expressionState.AddRoster(rosterNode.Identity.Id, rosterNode.Identity.RosterVector.Shrink(), rosterNode.Identity.RosterVector.Last(), rosterNode.SortIndex);
            }
        }

        private ILatestInterviewExpressionState GetClonedExpressionState()
        {
            ILatestInterviewExpressionState expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();
            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            return expressionProcessorState;
        }
    }
}