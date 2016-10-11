using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        private void UpdateExpressionState(InterviewTree sourceInterview, InterviewTree changedInterview, ILatestInterviewExpressionState expressionState)
        {
            var diff = sourceInterview.FindDiff(changedInterview);

            UpdateAnswers(diff, expressionState);
            UpdateRosters(diff, expressionState);
        }

        private static void UpdateAnswers(IReadOnlyCollection<InterviewTreeNodeDiff> diff, ILatestInterviewExpressionState expressionState)
        {
            var removedQuestionIdentitiesByRemovedRosters = diff
                .Where(x => x.SourceNode is InterviewTreeRoster && x.ChangedNode == null)
                .Select(x => x.SourceNode)
                .TreeToEnumerable(x => x.Children)
                .DistinctBy(x => x.Identity)
                .OfType<InterviewTreeQuestion>()
                .Select(x => x.Identity);

            var changedQuestions = diff
                .Where(IsQuestionWithChangedAnswer)
                .Select(x => x.ChangedNode)
                .Cast<InterviewTreeQuestion>();

            foreach (var changedQuestion in changedQuestions)
            {
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

            foreach (var removedQuestionIdentity in removedQuestionIdentitiesByRemovedRosters)
            {
                expressionState.RemoveAnswer(removedQuestionIdentity);
            }
        }

        private static void UpdateRosters(IReadOnlyCollection<InterviewTreeNodeDiff> diff, ILatestInterviewExpressionState expressionState)
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

        private static bool IsQuestionWithChangedAnswer(InterviewTreeNodeDiff diff)
        {
            var sourceQuestion = diff.SourceNode as InterviewTreeQuestion;
            var changedQuestion = diff.ChangedNode as InterviewTreeQuestion;

            if (sourceQuestion == null || changedQuestion == null) return false;

            if (sourceQuestion.IsText) return !sourceQuestion.AsText.EqualByAnswer(changedQuestion.AsText);
            if (sourceQuestion.IsInteger) return !sourceQuestion.AsInteger.EqualByAnswer(changedQuestion.AsInteger);
            if (sourceQuestion.IsDouble) return !sourceQuestion.AsDouble.EqualByAnswer(changedQuestion.AsDouble);
            if (sourceQuestion.IsDateTime) return !sourceQuestion.AsDateTime.EqualByAnswer(changedQuestion.AsDateTime);
            if (sourceQuestion.IsMultimedia) return !sourceQuestion.AsMultimedia.EqualByAnswer(changedQuestion.AsMultimedia);
            if (sourceQuestion.IsQRBarcode) return !sourceQuestion.AsQRBarcode.EqualByAnswer(changedQuestion.AsQRBarcode);
            if (sourceQuestion.IsGps) return !sourceQuestion.AsGps.EqualByAnswer(changedQuestion.AsGps);
            if (sourceQuestion.IsSingleOption) return !sourceQuestion.AsSingleOption.EqualByAnswer(changedQuestion.AsSingleOption);
            if (sourceQuestion.IsSingleLinkedOption) return !sourceQuestion.AsSingleLinkedOption.EqualByAnswer(changedQuestion.AsSingleLinkedOption);
            if (sourceQuestion.IsMultiOption) return !sourceQuestion.AsMultiOption.EqualByAnswer(changedQuestion.AsMultiOption);
            if (sourceQuestion.IsMultiLinkedOption) return !sourceQuestion.AsMultiLinkedOption.EqualByAnswer(changedQuestion.AsMultiLinkedOption);
            if (sourceQuestion.IsYesNo) return !sourceQuestion.AsYesNo.EqualByAnswer(changedQuestion.AsYesNo);
            if (sourceQuestion.IsTextList) return !sourceQuestion.AsTextList.EqualByAnswer(changedQuestion.AsTextList);

            return false;
        }
    }
}