using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V2
{
    internal class InterviewExpressionStateV1AdaptedToV2 : IInterviewExpressionStateV2
    {
        private readonly IInterviewExpressionState interviewExpressionState;

        public InterviewExpressionStateV1AdaptedToV2(IInterviewExpressionState interviewExpressionState)
        {
            this.interviewExpressionState = interviewExpressionState;
        }

        public void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer)
        {
            interviewExpressionState.UpdateNumericIntegerAnswer(questionId, rosterVector, answer);
        }

        public void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer)
        {
            interviewExpressionState.UpdateNumericRealAnswer(questionId, rosterVector, answer);
        }

        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer)
        {
            interviewExpressionState.UpdateDateAnswer(questionId, rosterVector, answer);
        }

        public void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            interviewExpressionState.UpdateMediaAnswer(questionId, rosterVector, answer);
        }

        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            interviewExpressionState.UpdateTextAnswer(questionId, rosterVector, answer);
        }

        public void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            interviewExpressionState.UpdateQrBarcodeAnswer(questionId, rosterVector, answer);
        }

        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer)
        {
            interviewExpressionState.UpdateSingleOptionAnswer(questionId, rosterVector, answer);
        }

        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            interviewExpressionState.UpdateMultiOptionAnswer(questionId, rosterVector, answer);
        }

        public void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude, double longitude,
            double accuracy, double altitude)
        {
            interviewExpressionState.UpdateGeoLocationAnswer(questionId, propagationVector, latitude, longitude, accuracy, altitude);
        }

        public void UpdateTextListAnswer(Guid questionId, decimal[] propagationVector, Tuple<decimal, string>[] answers)
        {
            interviewExpressionState.UpdateTextListAnswer(questionId, propagationVector, answers); 
        }

        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[] selectedPropagationVector)
        {
            interviewExpressionState.UpdateLinkedSingleOptionAnswer(questionId, propagationVector, selectedPropagationVector); 
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[][] selectedPropagationVectors)
        {
            interviewExpressionState.UpdateLinkedMultiOptionAnswer(questionId, propagationVector, selectedPropagationVectors); 
        }

        public void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions)
        {
            interviewExpressionState.DeclareAnswersInvalid(invalidQuestions); 
        }

        public void DeclareAnswersValid(IEnumerable<Identity> validQuestions)
        {
            interviewExpressionState.DeclareAnswersValid(validQuestions); 
        }

        public void DisableGroups(IEnumerable<Identity> groupsToDisable)
        {
            interviewExpressionState.DisableGroups(groupsToDisable); 
        }

        public void EnableGroups(IEnumerable<Identity> groupsToEnable)
        {
            interviewExpressionState.EnableGroups(groupsToEnable); 
        }

        public void DisableQuestions(IEnumerable<Identity> questionsToDisable)
        {
            interviewExpressionState.DisableQuestions(questionsToDisable); 
        }

        public void EnableQuestions(IEnumerable<Identity> questionsToEnable)
        {
            interviewExpressionState.EnableQuestions(questionsToEnable); 
        }

        public void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            interviewExpressionState.AddRoster(rosterId, outerRosterVector, rosterInstanceId, sortIndex); 
        }

        public void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId)
        {
            interviewExpressionState.RemoveRoster(rosterId, rosterVector, rosterInstanceId); 
        }

        public ValidityChanges ProcessValidationExpressions()
        {
            return interviewExpressionState.ProcessValidationExpressions(); 
        }

        public EnablementChanges ProcessEnablementConditions()
        {
            return interviewExpressionState.ProcessEnablementConditions(); 
        }

        public void SaveAllCurrentStatesAsPrevious()
        {
            interviewExpressionState.SaveAllCurrentStatesAsPrevious(); 
        }

        public IInterviewExpressionState Clone()
        {
            return ((IInterviewExpressionStateV2)interviewExpressionState).Clone(); 
        }

        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone()
        {
            return new InterviewExpressionStateV1AdaptedToV2(interviewExpressionState.Clone());
        }

        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle)
        {
        }
    }
}