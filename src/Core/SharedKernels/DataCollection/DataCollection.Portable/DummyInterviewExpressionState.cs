using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    /// <summary>
    /// This class is for testing loading generated dll file
    /// </summary>
    public class DummyInterviewExpressionState : IInterviewExpressionState
    {
        public void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude, double longitude, double accuracy,
            double altitude)
        {
            throw new NotImplementedException();
        }

        public void UpdateTextListAnswer(Guid questionId, decimal[] propagationVector, Tuple<decimal, string>[] answers)
        {
            throw new NotImplementedException();
        }

        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[] selectedPropagationVector)
        {
            throw new NotImplementedException();
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[][] selectedPropagationVectors)
        {
            throw new NotImplementedException();
        }

        public void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions)
        {
            throw new NotImplementedException();
        }

        public void DeclareAnswersValid(IEnumerable<Identity> validQuestions)
        {
            throw new NotImplementedException();
        }

        public void DisableGroups(IEnumerable<Identity> groupsToDisable)
        {
            throw new NotImplementedException();
        }

        public void EnableGroups(IEnumerable<Identity> groupsToEnable)
        {
            throw new NotImplementedException();
        }

        public void DisableQuestions(IEnumerable<Identity> questionsToDisable)
        {
            throw new NotImplementedException();
        }

        public void EnableQuestions(IEnumerable<Identity> questionsToEnable)
        {
            throw new NotImplementedException();
        }

        public void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            throw new NotImplementedException();
        }

        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle)
        {
            throw new NotImplementedException();
        }

        public void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId)
        {
            throw new NotImplementedException();
        }

        public ValidityChanges ProcessValidationExpressions()
        {
            throw new NotImplementedException();
        }

        public EnablementChanges ProcessEnablementConditions()
        {
            throw new NotImplementedException();
        }

        public void SaveAllCurrentStatesAsPrevious()
        {
            throw new NotImplementedException();
        }

        public IInterviewExpressionState Clone()
        {
            throw new NotImplementedException();
        }
    }
}
