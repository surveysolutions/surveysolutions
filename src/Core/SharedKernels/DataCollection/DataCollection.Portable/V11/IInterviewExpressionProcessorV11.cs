using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V11
{
    public interface IInterviewExpressionProcessorV11
    {
        // Fix
        void UpdateSingleOptionAnswer(Identity questionId, int answer);
        void UpdateMultiOptionAnswer(Identity questionId,  int[] answer);
        void UpdateTextListAnswer(Identity questionId,  ListAnswerRow[] answers);
        void UpdateLinkedSingleOptionAnswer(Identity questionId,  RosterVector selectedPropagationVector);
        void UpdateLinkedMultiOptionAnswer(Identity questionId, RosterVector[] selectedPropagationVectors);
        void UpdateYesNoAnswer(Identity questionId, YesNoAnswersOnlyV2 answers);

        // Maybe
        void UpdateNumericIntegerAnswer(Identity questionId, long? answer);

        // OK
        void UpdateNumericRealAnswer(Identity questionId, double? answer);
        void UpdateDateAnswer(Identity questionId, DateTime? answer);
        void UpdateMediaAnswer(Identity questionId, string answer);
        void UpdateTextAnswer(Identity questionId, string answer);
        void UpdateQrBarcodeAnswer(Identity questionId, string answer);
        void UpdateGeoLocationAnswer(Identity questionId,  double latitude, double longitude, double accuracy, double altitude);

        void DisableGroups(IEnumerable<Identity> groupsToDisable);
        void EnableGroups(IEnumerable<Identity> groupsToEnable);
        void DisableQuestions(IEnumerable<Identity> questionsToDisable);
        void EnableQuestions(IEnumerable<Identity> questionsToEnable);
        void DisableStaticTexts(IEnumerable<Identity> staticTextsToDisable);
        void EnableStaticTexts(IEnumerable<Identity> staticTextsToEnable);
        void DisableVariables(IEnumerable<Identity> variablesToDisable);
        void EnableVariables(IEnumerable<Identity> variablesToEnable);

        void AddRoster(Identity rosterId, int? sortIndex);
        void RemoveRoster(Identity rosterId);

        void SetInterviewProperties(IInterviewProperties properties);

        void UpdateVariableValue(Identity variableId, object value);

        IEnumerable<CategoricalOption> FilterOptionsForQuestion(Identity questionIdentity, IEnumerable<CategoricalOption> options);
        void RemoveAnswer(Identity questionIdentity);
    }
}