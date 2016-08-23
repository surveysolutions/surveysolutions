using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;
using WB.Core.SharedKernels.DataCollection.V8;
using WB.Core.SharedKernels.DataCollection.V9;
using WB.Core.SharedKernels.DataCollection.V10;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Adapters.V10
{
    public class InterviewExpressionStateV9ToV10Adapter : IInterviewExpressionStateV10
    {
        private readonly IInterviewExpressionStateV9 adaptee;

        public InterviewExpressionStateV9ToV10Adapter(IInterviewExpressionStateV9 adaptee)
        {
            this.adaptee = adaptee;
        }

        public static IInterviewExpressionStateV10 AdaptIfNeeded(IInterviewExpressionStateV9 adaptee)
            => adaptee as IInterviewExpressionStateV10 ?? new InterviewExpressionStateV9ToV10Adapter(adaptee);

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

        public VariableValueChanges ProcessVariables() => this.adaptee.ProcessVariables();
        public void DisableVariables(IEnumerable<Identity> variablesToDisable) => this.adaptee.DisableVariables(variablesToDisable);
        public void EnableVariables(IEnumerable<Identity> variablesToEnable) => this.adaptee.EnableVariables(variablesToEnable);
        public void UpdateVariableValue(Identity variableIdentity, object value) => this.adaptee.UpdateVariableValue(variableIdentity, value);

        public IEnumerable<CategoricalOption> FilterOptionsForQuestion(Identity questionIdentity, IEnumerable<CategoricalOption> options) => 
            options;

        public void RemoveAnswer(Identity questionIdentity)
        {
            this.adaptee.UpdateNumericIntegerAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateNumericRealAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateDateAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateMediaAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateTextAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateQrBarcodeAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateSingleOptionAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateMultiOptionAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateGeoLocationAnswer(questionIdentity.Id, questionIdentity.RosterVector, 0, 0, 0, 0);
            this.adaptee.UpdateTextListAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateLinkedSingleOptionAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateLinkedMultiOptionAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
            this.adaptee.UpdateYesNoAnswer(questionIdentity.Id, questionIdentity.RosterVector, null);
        }

        public void RemoveRosterAndItsDependencies(Identity[] rosterKey, Guid rosterSorceId, decimal rosterInstanceId) { }

        public StructuralChanges GetStructuralChanges() => new StructuralChanges();

        IInterviewExpressionState IInterviewExpressionState.Clone() => this.Clone();
        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone() => this.Clone();
        IInterviewExpressionStateV4 IInterviewExpressionStateV4.Clone() => this.Clone();
        IInterviewExpressionStateV5 IInterviewExpressionStateV5.Clone() => this.Clone();
        IInterviewExpressionStateV6 IInterviewExpressionStateV6.Clone() => this.Clone();
        IInterviewExpressionStateV7 IInterviewExpressionStateV7.Clone() => this.Clone();
        IInterviewExpressionStateV8 IInterviewExpressionStateV8.Clone() => this.Clone();
        IInterviewExpressionStateV9 IInterviewExpressionStateV9.Clone() => this.Clone();
        IInterviewExpressionStateV10 IInterviewExpressionStateV10.Clone() => this.Clone();
        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle)
        {
             this.adaptee.UpdateRosterTitle(rosterId, outerRosterVector, rosterInstanceId, rosterTitle);
        }

        public void SetInterviewProperties(IInterviewProperties properties) => this.adaptee.SetInterviewProperties(properties);

        public void ApplyFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions) => this.adaptee.ApplyFailedValidations(failedValidationConditions);

        private IInterviewExpressionStateV10 Clone() => new InterviewExpressionStateV9ToV10Adapter(this.adaptee.Clone());
    }
}