using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;
using WB.Core.SharedKernels.DataCollection.V8;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.Latest
{
    public class InterviewExpressionStateV8ToLatestAdapter : ILatestInterviewExpressionState
    {
        private readonly IInterviewExpressionStateV8 adaptee;

        public InterviewExpressionStateV8ToLatestAdapter(IInterviewExpressionStateV8 adaptee)
        {
            this.adaptee = adaptee;
        }

        public static ILatestInterviewExpressionState AdaptIfNeeded(IInterviewExpressionStateV8 adaptee)
            => adaptee as ILatestInterviewExpressionState ?? new InterviewExpressionStateV8ToLatestAdapter(adaptee);

        public void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer) => this.adaptee.UpdateNumericIntegerAnswer(questionId, rosterVector, answer);
        public void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer) => this.adaptee.UpdateNumericRealAnswer(questionId, rosterVector, answer);
        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer) => this.adaptee.UpdateDateAnswer(questionId, rosterVector, answer);
        public void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer) => this.adaptee.UpdateMediaAnswer(questionId, rosterVector, answer);
        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer) => this.adaptee.UpdateTextAnswer(questionId, rosterVector, answer);
        public void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer) => this.adaptee.UpdateQrBarcodeAnswer(questionId, rosterVector, answer);
        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer) => this.adaptee.UpdateSingleOptionAnswer(questionId, rosterVector, answer);
        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer) => this.adaptee.UpdateMultiOptionAnswer(questionId, rosterVector, answer);
        public void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude, double longitude, double accuracy, double altitude) => this.adaptee.UpdateGeoLocationAnswer(questionId, propagationVector, latitude, longitude, accuracy, altitude);
        public void UpdateTextListAnswer(Guid questionId, decimal[] propagationVector, Tuple<decimal, string>[] answers) => this.adaptee.UpdateTextListAnswer(questionId, propagationVector, answers);
        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[] selectedPropagationVector) => this.adaptee.UpdateLinkedSingleOptionAnswer(questionId, propagationVector, selectedPropagationVector);
        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[][] selectedPropagationVectors) => this.adaptee.UpdateLinkedMultiOptionAnswer(questionId, propagationVector, selectedPropagationVectors);
        public void UpdateYesNoAnswer(Guid questionId, decimal[] propagationVector, YesNoAnswersOnly selectedPropagationVectors) => this.adaptee.UpdateYesNoAnswer(questionId, propagationVector, selectedPropagationVectors);
        public void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions) => this.adaptee.DeclareAnswersInvalid(invalidQuestions);
        public void DeclareAnswersValid(IEnumerable<Identity> validQuestions) => this.adaptee.DeclareAnswersValid(validQuestions);
        public void DisableGroups(IEnumerable<Identity> groupsToDisable) => this.adaptee.DisableGroups(groupsToDisable);
        public void EnableGroups(IEnumerable<Identity> groupsToEnable) => this.adaptee.EnableGroups(groupsToEnable);
        public void DisableQuestions(IEnumerable<Identity> questionsToDisable) => this.adaptee.DisableQuestions(questionsToDisable);
        public void EnableQuestions(IEnumerable<Identity> questionsToEnable) => this.adaptee.EnableQuestions(questionsToEnable);
        public void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex) => this.adaptee.AddRoster(rosterId, outerRosterVector, rosterInstanceId, sortIndex);
        public void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId) => this.adaptee.RemoveRoster(rosterId, rosterVector, rosterInstanceId);
        public ValidityChanges ProcessValidationExpressions() => this.adaptee.ProcessValidationExpressions();
        public EnablementChanges ProcessEnablementConditions() => this.adaptee.ProcessEnablementConditions();
        public void SaveAllCurrentStatesAsPrevious() => this.adaptee.SaveAllCurrentStatesAsPrevious();
        public LinkedQuestionOptionsChanges ProcessLinkedQuestionFilters() => this.adaptee.ProcessLinkedQuestionFilters();
        public bool AreLinkedQuestionsSupported() => this.adaptee.AreLinkedQuestionsSupported();
        public void DisableStaticTexts(IEnumerable<Identity> staticTextsToDisable) => this.adaptee.DisableStaticTexts(staticTextsToDisable);
        public void EnableStaticTexts(IEnumerable<Identity> staticTextsToEnable) => this.adaptee.EnableStaticTexts(staticTextsToEnable);
        IInterviewExpressionState IInterviewExpressionState.Clone() => this.Clone();
        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone() => this.Clone();
        IInterviewExpressionStateV4 IInterviewExpressionStateV4.Clone() => this.Clone();
        IInterviewExpressionStateV5 IInterviewExpressionStateV5.Clone() => this.Clone();
        IInterviewExpressionStateV6 IInterviewExpressionStateV6.Clone() => this.Clone();
        IInterviewExpressionStateV7 IInterviewExpressionStateV7.Clone() => this.Clone();
        IInterviewExpressionStateV8 IInterviewExpressionStateV8.Clone() => this.Clone();
        ILatestInterviewExpressionState ILatestInterviewExpressionState.Clone() => this.Clone();
        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle) => this.adaptee.UpdateRosterTitle(rosterId, outerRosterVector, rosterInstanceId, rosterTitle);
        public void SetInterviewProperties(IInterviewProperties properties) => this.adaptee.SetInterviewProperties(properties);
        public void ApplyFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions) => this.adaptee.DeclareAnswersInvalid(failedValidationConditions.Keys);

        private ILatestInterviewExpressionState Clone() => new InterviewExpressionStateV8ToLatestAdapter(this.adaptee.Clone());
    }
}