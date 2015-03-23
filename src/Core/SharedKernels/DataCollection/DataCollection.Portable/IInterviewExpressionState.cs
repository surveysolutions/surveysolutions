using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewExpressionState
    {
        void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer);
        void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer);
        void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer);
        void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer);
        void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer);
        void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer);
        void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer);
        void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer);
        void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude, double longitude, double accuracy, double altitude);
        void UpdateTextListAnswer(Guid questionId, decimal[] propagationVector, Tuple<decimal, string>[] answers);
        void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[] selectedPropagationVector);
        void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[][] selectedPropagationVectors);

        void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions);
        void DeclareAnswersValid(IEnumerable<Identity> validQuestions);
        
        void DisableGroups(IEnumerable<Identity> groupsToDisable);
        void EnableGroups(IEnumerable<Identity> groupsToEnable);
        void DisableQuestions(IEnumerable<Identity> questionsToDisable);
        void EnableQuestions(IEnumerable<Identity> questionsToEnable);
        
        void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex);
        void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle);
        void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId);

        ValidityChanges ProcessValidationExpressions();
        EnablementChanges ProcessEnablementConditions();
        void SaveAllCurrentStatesAsPrevious();

        IInterviewExpressionState Clone();
    }
}