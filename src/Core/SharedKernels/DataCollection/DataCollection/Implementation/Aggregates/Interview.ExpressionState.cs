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
        }

        private static void UpdateAnswersInExpressionState(IReadOnlyCollection<InterviewTreeNodeDiff> diff, ILatestInterviewExpressionState expressionState)
        {
            var diffByChangedQuestions = diff
                .Where(x=>x.SourceNode is InterviewTreeQuestion || x.ChangedNode is InterviewTreeQuestion);

            foreach (var diffByQuestion in diffByChangedQuestions)
            {
                var sourceQuestion = diffByQuestion.SourceNode as InterviewTreeQuestion;
                var changedQuestion = diffByQuestion.ChangedNode as InterviewTreeQuestion;

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

                if (changedQuestion.IsDouble && changedQuestion.AsDouble.IsAnswered)
                {
                    expressionState.UpdateNumericRealAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsDouble.GetAnswer());
                }

                if (changedQuestion.IsInteger && changedQuestion.AsInteger.IsAnswered)
                {
                    expressionState.UpdateNumericIntegerAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsInteger.GetAnswer());
                }

                if (changedQuestion.IsDateTime && changedQuestion.AsDateTime.IsAnswered)
                {
                    expressionState.UpdateDateAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsDateTime.GetAnswer());
                }

                if (changedQuestion.IsGps && changedQuestion.AsGps.IsAnswered)
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

                if (changedQuestion.IsYesNo && changedQuestion.AsYesNo.IsAnswered)
                {
                    expressionState.UpdateYesNoAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, ConvertToYesNoAnswersOnly(changedQuestion.AsYesNo.GetAnswer()));
                }

                if (changedQuestion.IsSingleOption && changedQuestion.AsSingleOption.IsAnswered)
                {
                    expressionState.UpdateSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsSingleOption.GetAnswer());
                }

                if (changedQuestion.IsSingleOption && changedQuestion.AsSingleOption.IsAnswered)
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
            var removedRosters =
                diff.Where(x => x.SourceNode is InterviewTreeRoster && x.ChangedNode == null)
                    .Select(x => x.SourceNode)
                    .ToArray();

            var addedRosters =
                diff.Where(x => x.SourceNode == null && x.ChangedNode is InterviewTreeRoster)
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