using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V7
{
    internal class InterviewExpressionStateV6ToV7Adapter : IInterviewExpressionStateV7
    {
        private readonly IInterviewExpressionStateV6 interviewExpressionState;

        public InterviewExpressionStateV6ToV7Adapter(IInterviewExpressionStateV6 interviewExpressionState)
        {
            this.interviewExpressionState = interviewExpressionState;
        }

        public static IInterviewExpressionStateV7 AdaptIfNeeded(IInterviewExpressionStateV6 adaptee)
            => adaptee as IInterviewExpressionStateV7 ?? new InterviewExpressionStateV6ToV7Adapter(adaptee);

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

        public void UpdateYesNoAnswer(Guid questionId, decimal[] propagationVector, YesNoAnswersOnly selectedPropagationVectors)
        {
            this.interviewExpressionState.UpdateYesNoAnswer(questionId, propagationVector, selectedPropagationVectors);
        }

        public void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions)
        {
            //code should be here
            this.interviewExpressionState.DeclareAnswersInvalid(invalidQuestions); 
        }

        public void DeclareAnswersValid(IEnumerable<Identity> validQuestions)
        {
            //code should be here
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
            //code should be put here
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

        public LinkedQuestionOptionsChanges ProcessLinkedQuestionFilters()
        {
            throw new NotSupportedException("linked questions are not supported by versions lower then V7");
        }

        public bool AreLinkedQuestionsSupported()
        {
            return false;
        }

        IInterviewExpressionState IInterviewExpressionState.Clone() => this.Clone();
        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone() => this.Clone();
        IInterviewExpressionStateV4 IInterviewExpressionStateV4.Clone() => this.Clone();
        IInterviewExpressionStateV5 IInterviewExpressionStateV5.Clone() => this.Clone();
        IInterviewExpressionStateV6 IInterviewExpressionStateV6.Clone() => this.Clone();
        IInterviewExpressionStateV7 IInterviewExpressionStateV7.Clone() => this.Clone();

        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle)
        {
            this.interviewExpressionState.UpdateRosterTitle(rosterId, outerRosterVector, rosterInstanceId, rosterTitle);
        }

        public void SetInterviewProperties(IInterviewProperties properties)
        {
            this.interviewExpressionState.SetInterviewProperties(properties);
        }

        public void ApplyFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions)
        {
            this.interviewExpressionState.ApplyFailedValidations(failedValidationConditions);
        }

        private IInterviewExpressionStateV7 Clone() => new InterviewExpressionStateV6ToV7Adapter(this.interviewExpressionState.Clone());
    }
}