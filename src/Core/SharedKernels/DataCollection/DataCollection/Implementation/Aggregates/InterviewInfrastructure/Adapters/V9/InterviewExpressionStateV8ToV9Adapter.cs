using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.Latest;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;
using WB.Core.SharedKernels.DataCollection.V8;
using WB.Core.SharedKernels.DataCollection.V9;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V9
{
    public class InterviewExpressionStateV8ToV9Adapter : IInterviewExpressionStateV9
    {
        private readonly IInterviewExpressionStateV8 adaptee;

        public InterviewExpressionStateV8ToV9Adapter(IInterviewExpressionStateV8 adaptee)
        {
            this.adaptee = adaptee;
        }

        public static IInterviewExpressionStateV9 AdaptIfNeeded(IInterviewExpressionStateV8 adaptee)
            => adaptee as IInterviewExpressionStateV9 ?? new InterviewExpressionStateV8ToV9Adapter(adaptee);

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

        public void DeclareStaticTextValid(IEnumerable<Identity> validQuestions) => this.adaptee.DeclareStaticTextValid(validQuestions);
        
        public void ApplyStaticTextFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions)
            => this.adaptee.ApplyStaticTextFailedValidations(failedValidationConditions);

        public VariableValueChanges ProcessVariables()
        {
            return  new VariableValueChanges();
        }

        public void DisableVariables(IEnumerable<Identity> variablesToDisable)
        {
            throw new NotImplementedException("Variables are missing is questionnairies older then V9, it's nothing to disable");
        }

        public void EnableVariables(IEnumerable<Identity> variablesToEnable)
        {
            throw new NotImplementedException("Variables are missing is questionnairies older then V9, it's nothing to enable");
        }

        public void UpdateVariableValue(Identity variableIdentity, object value)
        {
            throw new NotImplementedException("Variables are missing is questionnairies older then V9, it's nothing to set");
        }

        IInterviewExpressionState IInterviewExpressionState.Clone() => this.Clone();
        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone() => this.Clone();
        IInterviewExpressionStateV4 IInterviewExpressionStateV4.Clone() => this.Clone();
        IInterviewExpressionStateV5 IInterviewExpressionStateV5.Clone() => this.Clone();
        IInterviewExpressionStateV6 IInterviewExpressionStateV6.Clone() => this.Clone();
        IInterviewExpressionStateV7 IInterviewExpressionStateV7.Clone() => this.Clone();
        IInterviewExpressionStateV8 IInterviewExpressionStateV8.Clone() => this.Clone();
        IInterviewExpressionStateV9 IInterviewExpressionStateV9.Clone() => this.Clone();

        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle)
        {
            this.adaptee.UpdateRosterTitle(rosterId, outerRosterVector, rosterInstanceId, rosterTitle);
        }

        public void SetInterviewProperties(IInterviewProperties properties) => this.adaptee.SetInterviewProperties(properties);

        public void ApplyFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions) => this.adaptee.ApplyFailedValidations(failedValidationConditions);

        private IInterviewExpressionStateV9 Clone() => new InterviewExpressionStateV8ToV9Adapter(this.adaptee.Clone());
    }
}