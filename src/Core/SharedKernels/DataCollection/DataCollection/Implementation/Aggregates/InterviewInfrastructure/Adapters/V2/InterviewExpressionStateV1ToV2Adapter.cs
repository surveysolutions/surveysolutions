using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V2
{
    internal class InterviewExpressionStateV1ToV2Adapter : IInterviewExpressionStateV2
    {
        private readonly IInterviewExpressionState interviewExpressionState;

        public InterviewExpressionStateV1ToV2Adapter(IInterviewExpressionState interviewExpressionState)
        {
            this.interviewExpressionState = interviewExpressionState;
        }

        public static IInterviewExpressionStateV2 AdaptIfNeeded(IInterviewExpressionState adaptee)
            => adaptee as IInterviewExpressionStateV2 ?? new InterviewExpressionStateV1ToV2Adapter(adaptee);

        public void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer)
        {
            this.interviewExpressionState.UpdateNumericIntegerAnswer(questionId, rosterVector, answer);
        }

        public void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer)
        {
            this.interviewExpressionState.UpdateNumericRealAnswer(questionId, rosterVector, answer);
        }

        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer)
        {
            this.interviewExpressionState.UpdateDateAnswer(questionId, rosterVector, answer);
        }

        public void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            this.interviewExpressionState.UpdateMediaAnswer(questionId, rosterVector, answer);
        }

        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            this.interviewExpressionState.UpdateTextAnswer(questionId, rosterVector, answer);
        }

        public void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            this.interviewExpressionState.UpdateQrBarcodeAnswer(questionId, rosterVector, answer);
        }

        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer)
        {
            this.interviewExpressionState.UpdateSingleOptionAnswer(questionId, rosterVector, answer);
        }

        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            this.interviewExpressionState.UpdateMultiOptionAnswer(questionId, rosterVector, answer);
        }

        public void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude, double longitude,
            double accuracy, double altitude)
        {
            this.interviewExpressionState.UpdateGeoLocationAnswer(questionId, propagationVector, latitude, longitude, accuracy, altitude);
        }

        public void UpdateTextListAnswer(Guid questionId, decimal[] propagationVector, Tuple<decimal, string>[] answers)
        {
            this.interviewExpressionState.UpdateTextListAnswer(questionId, propagationVector, answers); 
        }

        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[] selectedPropagationVector)
        {
            this.interviewExpressionState.UpdateLinkedSingleOptionAnswer(questionId, propagationVector, selectedPropagationVector); 
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[][] selectedPropagationVectors)
        {
            this.interviewExpressionState.UpdateLinkedMultiOptionAnswer(questionId, propagationVector, selectedPropagationVectors); 
        }

        public void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions)
        {
            this.interviewExpressionState.DeclareAnswersInvalid(invalidQuestions); 
        }

        public void DeclareAnswersValid(IEnumerable<Identity> validQuestions)
        {
            this.interviewExpressionState.DeclareAnswersValid(validQuestions); 
        }

        public void DisableGroups(IEnumerable<Identity> groupsToDisable)
        {
            this.interviewExpressionState.DisableGroups(groupsToDisable); 
        }

        public void EnableGroups(IEnumerable<Identity> groupsToEnable)
        {
            this.interviewExpressionState.EnableGroups(groupsToEnable); 
        }

        public void DisableQuestions(IEnumerable<Identity> questionsToDisable)
        {
            this.interviewExpressionState.DisableQuestions(questionsToDisable); 
        }

        public void EnableQuestions(IEnumerable<Identity> questionsToEnable)
        {
            this.interviewExpressionState.EnableQuestions(questionsToEnable); 
        }

        public void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            this.interviewExpressionState.AddRoster(rosterId, outerRosterVector, rosterInstanceId, sortIndex); 
        }

        public void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId)
        {
            this.interviewExpressionState.RemoveRoster(rosterId, rosterVector, rosterInstanceId); 
        }

        public ValidityChanges ProcessValidationExpressions()
        {
            return this.interviewExpressionState.ProcessValidationExpressions(); 
        }

        public EnablementChanges ProcessEnablementConditions()
        {
            return this.interviewExpressionState.ProcessEnablementConditions(); 
        }

        public void SaveAllCurrentStatesAsPrevious()
        {
            this.interviewExpressionState.SaveAllCurrentStatesAsPrevious(); 
        }

        public IInterviewExpressionState Clone()
        {
            return ((IInterviewExpressionStateV2)this.interviewExpressionState).Clone(); 
        }

        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone()
        {
            return new InterviewExpressionStateV1ToV2Adapter(this.interviewExpressionState.Clone());
        }

        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle)
        {
        }
    }
}