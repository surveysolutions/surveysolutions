using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IExpressionExecutable
    {
        IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances);
        Identity[] GetRosterKey();
        void SetParent(IExpressionExecutable parentLevel);
        IExpressionExecutable GetParent();
        IExpressionExecutable CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey);

        int GetLevel();

        void CalculateValidationChanges(out List<Identity> questionsToBeValid, out List<Identity> questionsToBeInvalid);

        void CalculateConditionChanges(out List<Identity> questionsToBeEnabled, out List<Identity> questionsToBeDisabled,
            out List<Identity> groupsToBeEnabled, out List<Identity> groupsToBeDisabled);

        void DisableQuestion(Guid questionId);
        void EnableQuestion(Guid questionId);

        void DisableGroup(Guid groupId);
        void EnableGroup(Guid groupId);

        void DeclareAnswerValid(Guid questionId);
        void DeclareAnswerInvalid(Guid questionId);

        void UpdateNumericIntegerAnswer(Guid questionId, long? answer);
        void UpdateNumericRealAnswer(Guid questionId, double? answer);
        void UpdateDateTimeAnswer(Guid questionId, DateTime? answer);
        void UpdateTextAnswer(Guid questionId, string answer);
        void UpdateMediaAnswer(Guid questionId, string answer);
        void UpdateQrBarcodeAnswer(Guid questionId, string answer);
        void UpdateSingleOptionAnswer(Guid questionId, decimal? answer);
        void UpdateMultiOptionAnswer(Guid questionId, decimal[] answer);
        void UpdateGeoLocationAnswer(Guid questionId, double latitude, double longitude, double precision, double altitude);
        void UpdateTextListAnswer(Guid questionId, Tuple<decimal, string>[] answers);
        void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] selectedPropagationVector);
        void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[][] selectedPropagationVectors);
        void SaveAllCurrentStatesAsPrevious();
    }
}