using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public interface IInterviewExpressionState
    {
        void UpdateIntAnswer(Guid questionId, decimal[] rosterVector, long answer);
        void UpdateDecimalAnswer(Guid questionId, decimal[] rosterVector, decimal answer);
        void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime answer);
        void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer);
        void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer);
        void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal answer);
        void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer);
        void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude, double longitude);
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
        void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId);


        void ProcessValidationExpressions(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);
        void ProcessConditionExpressions(List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled, List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled);

        IInterviewExpressionState Clone();
    }
}