using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        private void UpdateExpressionState(InterviewTree sourceInterview, InterviewTree changedInterview, ILatestInterviewExpressionState expressionState)
        {
            var diff = sourceInterview.Compare(changedInterview);

            UpdateAnswersInExpressionState(diff, expressionState);
            UpdateRostersInExpressionState(diff, expressionState);
            UpdateDisablingInExpressionState(diff, expressionState);
        }

        private void UpdateDisablingInExpressionState(IReadOnlyCollection<InterviewTreeNodeDiff> diff, ILatestInterviewExpressionState expressionState)
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

            if (disabledGroups.Any()) expressionState.DisableGroups(disabledGroups);
            if (enabledGroups.Any()) expressionState.EnableGroups(enabledGroups);
            if (disabledQuestions.Any()) expressionState.DisableQuestions(disabledQuestions);
            if (enabledQuestions.Any()) expressionState.EnableQuestions(enabledQuestions);
            if (disabledStaticTexts.Any()) expressionState.DisableStaticTexts(disabledStaticTexts);
            if (enabledStaticTexts.Any()) expressionState.EnableStaticTexts(enabledStaticTexts);
            if (disabledVariables.Any()) expressionState.DisableVariables(disabledVariables);
            if (enabledVariables.Any()) expressionState.EnableVariables(enabledVariables);
        }

        private static void UpdateAnswersInExpressionState(IReadOnlyCollection<InterviewTreeNodeDiff> diff, ILatestInterviewExpressionState expressionState)
        {
            foreach (var diffByQuestion in diff.OfType<InterviewTreeQuestionDiff>())
            {
                var sourceQuestion = diffByQuestion.SourceNode;
                var changedQuestion = diffByQuestion.ChangedNode;

                if (sourceQuestion == null) continue;

                if (IsAnswerRemoved(diffByQuestion))
                {
                    expressionState.RemoveAnswer(sourceQuestion.Identity);
                    continue;
                }

                if (changedQuestion == null) continue;

                if (changedQuestion.IsText)
                {
                    expressionState.UpdateTextAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsText.GetAnswer());
                }

                if (changedQuestion.IsTextList)
                {
                    expressionState.UpdateTextListAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsTextList.GetAnswer());
                }

                if (changedQuestion.IsDouble)
                {
                    expressionState.UpdateNumericRealAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsDouble.GetAnswer());
                }

                if (changedQuestion.IsInteger)
                {
                    expressionState.UpdateNumericIntegerAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsInteger.GetAnswer());
                }

                if (changedQuestion.IsDateTime)
                {
                    expressionState.UpdateDateAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsDateTime.GetAnswer());
                }

                if (changedQuestion.IsGps)
                {
                    var gpsAnswer = changedQuestion.AsGps.GetAnswer();
                    expressionState.UpdateGeoLocationAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, gpsAnswer.Latitude, gpsAnswer.Longitude,
                        gpsAnswer.Accuracy, gpsAnswer.Altitude);
                }

                if (changedQuestion.IsQRBarcode)
                {
                    expressionState.UpdateQrBarcodeAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsQRBarcode.GetAnswer());
                }

                if (changedQuestion.IsMultimedia)
                {
                    expressionState.UpdateMediaAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsMultimedia.GetAnswer());
                }

                if (changedQuestion.IsYesNo)
                {
                    expressionState.UpdateYesNoAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, ConvertToYesNoAnswersOnly(changedQuestion.AsYesNo.GetAnswer()));
                }

                if (changedQuestion.IsSingleOption)
                {
                    expressionState.UpdateSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsSingleOption.GetAnswer());
                }

                if (changedQuestion.IsSingleOption)
                {
                    expressionState.UpdateSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsSingleOption.GetAnswer());
                }

                if (changedQuestion.IsMultiOption)
                {
                    expressionState.UpdateMultiOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsMultiOption.GetAnswer());
                }

                if (changedQuestion.IsSingleLinkedOption)
                {
                    expressionState.UpdateLinkedSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsSingleLinkedOption.GetAnswer());
                }

                if (changedQuestion.IsMultiLinkedOption)
                {
                    expressionState.UpdateLinkedSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsMultiLinkedOption.GetAnswer());
                }
            }
        }

        private static void UpdateRostersInExpressionState(IReadOnlyCollection<InterviewTreeNodeDiff> diff, ILatestInterviewExpressionState expressionState)
        {
            var removedRosters = diff
                .OfType<InterviewTreeRosterDiff>()
                .Where(x => x.SourceNode != null && x.ChangedNode == null)
                .Select(x => x.SourceNode)
                .ToArray();

            var addedRosters = diff
                .OfType<InterviewTreeRosterDiff>()
                .Where(x => x.SourceNode == null && x.ChangedNode != null)
                .Select(x => x.ChangedNode)
                .ToArray();

            foreach (var removedRosterIdentity in removedRosters.Select(ToRosterInstance))
            {
                expressionState.RemoveRoster(removedRosterIdentity.GroupId, removedRosterIdentity.OuterRosterVector, removedRosterIdentity.RosterInstanceId);
            }

            foreach (var addedRosterIdentity in addedRosters.Select(ToRosterInstance))
            {
                expressionState.AddRoster(addedRosterIdentity.GroupId, addedRosterIdentity.OuterRosterVector, addedRosterIdentity.RosterInstanceId, 0);
            }
        }

        private ILatestInterviewExpressionState GetClonedExpressionState()
        {
            ILatestInterviewExpressionState expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();
            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            return expressionProcessorState;
        }

        private static YesNoAnswersOnly ConvertToYesNoAnswersOnly(AnsweredYesNoOption[] answeredOptions)
        {
            var yesAnswers = answeredOptions.Where(x => x.Yes).Select(x => x.OptionValue).ToArray();
            var noAnswers = answeredOptions.Where(x => !x.Yes).Select(x => x.OptionValue).ToArray();
            return new YesNoAnswersOnly(yesAnswers, noAnswers);
        }
    }
}