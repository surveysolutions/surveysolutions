using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V11
{
    public interface IInterviewLevelV11
    {
        IEnumerable<CategoricalOption> FilterOptionsForQuestion(Guid questionId, IEnumerable<CategoricalOption> options);

        decimal[] RosterVector { get; }

        void RemoveAnswer(Guid questionId);

        void EnableVariable(Guid variableId);
        void DisableVariable(Guid variableId);
        void UpdateVariableValue(Guid variableId, object value);

        void DisableStaticText(Guid staticTextId);
        void EnableStaticText(Guid staticTextId);

        void SetInterviewProperties(IInterviewProperties interviewProperties);
        
        Identity[] GetRosterKey();
        
        int GetLevel();

        void DisableQuestion(Guid questionId);
        void EnableQuestion(Guid questionId);

        void DisableGroup(Guid groupId);
        void EnableGroup(Guid groupId);

        void UpdateNumericIntegerAnswer(Guid questionId, long? answer);
        void UpdateNumericRealAnswer(Guid questionId, double? answer);
        void UpdateDateTimeAnswer(Guid questionId, DateTime? answer);
        void UpdateTextAnswer(Guid questionId, string answer);
        void UpdateMediaAnswer(Guid questionId, string answer);
        void UpdateQrBarcodeAnswer(Guid questionId, string answer);
        void UpdateSingleOptionAnswer(Guid questionId, int? answer);
        void UpdateMultiOptionAnswer(Guid questionId, int[] answer);
        void UpdateGeoLocationAnswer(Guid questionId, double latitude, double longitude, double precision, double altitude);
        void UpdateTextListAnswer(Guid questionId, ListAnswerRow[] answers);
        void UpdateLinkedSingleOptionAnswer(Guid questionId, RosterVector[] selectedPropagationVector);
        void UpdateLinkedMultiOptionAnswer(Guid questionId, RosterVector[] selectedPropagationVectors);
        void UpdateYesNoAnswer(Guid questionId, YesNoAnswersOnlyV2 answers);
    }
}
