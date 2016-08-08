using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;
using WB.Core.SharedKernels.DataCollection.V8;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V8
{
    internal class InterviewExpressionStateV7ToV8Adapter : IInterviewExpressionStateV8
    {
        private readonly IInterviewExpressionStateV7 adaptee;

        public InterviewExpressionStateV7ToV8Adapter(IInterviewExpressionStateV7 adaptee)
        {
            this.adaptee = adaptee;
        }

        public static IInterviewExpressionStateV8 AdaptIfNeeded(IInterviewExpressionStateV7 adaptee)
            => adaptee as IInterviewExpressionStateV8 ?? new InterviewExpressionStateV7ToV8Adapter(adaptee);

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
        
        IInterviewExpressionStateV8 IInterviewExpressionStateV8.Clone() => this.Clone();
        IInterviewExpressionStateV7 IInterviewExpressionStateV7.Clone() => this.Clone();
        public LinkedQuestionOptionsChanges ProcessLinkedQuestionFilters() => this.adaptee.ProcessLinkedQuestionFilters();
        public bool AreLinkedQuestionsSupported() => this.adaptee.AreLinkedQuestionsSupported();
        IInterviewExpressionStateV6 IInterviewExpressionStateV6.Clone() => this.Clone();
        public void ApplyFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions) => this.adaptee.ApplyFailedValidations(failedValidationConditions);
        IInterviewExpressionStateV5 IInterviewExpressionStateV5.Clone() => this.Clone();
        public void UpdateYesNoAnswer(Guid questionId, decimal[] propagationVector, YesNoAnswersOnly answers) => this.adaptee.UpdateYesNoAnswer(questionId, propagationVector, answers);
        IInterviewExpressionStateV4 IInterviewExpressionStateV4.Clone() => this.Clone();
        public void SetInterviewProperties(IInterviewProperties properties) => this.adaptee.SetInterviewProperties(properties);
        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone() => this.Clone();

        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle)
        {
            this.adaptee.UpdateRosterTitle(rosterId, outerRosterVector, rosterInstanceId, rosterTitle);
        }

        IInterviewExpressionState IInterviewExpressionState.Clone() => this.Clone();

        public void DisableStaticTexts(IEnumerable<Identity> staticTextsToDisable) { }
        public void EnableStaticTexts(IEnumerable<Identity> staticTextsToEnable) { }
        public void DeclareStaticTextValid(IEnumerable<Identity> validStaticTexts) {}
        public void ApplyStaticTextFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions) {}

        private IInterviewExpressionStateV8 Clone() => new InterviewExpressionStateV7ToV8Adapter(this.adaptee.Clone());
    }
}