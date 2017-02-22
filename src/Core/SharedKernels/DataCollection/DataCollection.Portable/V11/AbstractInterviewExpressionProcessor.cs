using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.V11
{
    public abstract class AbstractInterviewExpressionProcessorV11<T> : IInterviewExpressionProcessorV11
        where T : IInterviewLevelV11
    {
        public void UpdateSingleOptionAnswer(Identity questionId, int answer)
        {
        }

        public void UpdateMultiOptionAnswer(Identity questionId, int[] answer)
        {
        }

        public void UpdateTextListAnswer(Identity questionId, ListAnswerRow[] answers)
        {
        }

        public void UpdateLinkedSingleOptionAnswer(Identity questionId, RosterVector selectedPropagationVector)
        {
        }

        public void UpdateLinkedMultiOptionAnswer(Identity questionId, RosterVector[] selectedPropagationVectors)
        {
        }

        public void UpdateYesNoAnswer(Identity questionId, YesNoAnswersOnlyV2 answers)
        {
        }

        public void UpdateNumericIntegerAnswer(Identity questionId, long? answer)
        {
        }

        public void UpdateNumericRealAnswer(Identity questionId, double? answer)
        {
        }

        public void UpdateDateAnswer(Identity questionId, DateTime? answer)
        {
        }

        public void UpdateMediaAnswer(Identity questionId, string answer)
        {
        }

        public void UpdateTextAnswer(Identity questionId, string answer)
        {
        }

        public void UpdateQrBarcodeAnswer(Identity questionId, string answer)
        {
        }

        public void UpdateGeoLocationAnswer(Identity questionId, double latitude, double longitude, double accuracy, double altitude)
        {
        }

        public void DisableGroups(IEnumerable<Identity> groupsToDisable)
        {
        }

        public void EnableGroups(IEnumerable<Identity> groupsToEnable)
        {
        }

        public void DisableQuestions(IEnumerable<Identity> questionsToDisable)
        {
        }

        public void EnableQuestions(IEnumerable<Identity> questionsToEnable)
        {
        }

        public void DisableStaticTexts(IEnumerable<Identity> staticTextsToDisable)
        {
        }

        public void EnableStaticTexts(IEnumerable<Identity> staticTextsToEnable)
        {
        }

        public void DisableVariables(IEnumerable<Identity> variablesToDisable)
        {
        }

        public void EnableVariables(IEnumerable<Identity> variablesToEnable)
        {
        }

        public void AddRoster(Identity rosterId, int? sortIndex)
        {
        }

        public void RemoveRoster(Identity rosterId)
        {
        }

        public void SetInterviewProperties(IInterviewProperties properties)
        {
        }

        public void UpdateVariableValue(Identity variableId, object value)
        {
        }

        public IEnumerable<CategoricalOption> FilterOptionsForQuestion(Identity questionIdentity, IEnumerable<CategoricalOption> options)
        {
            return Enumerable.Empty<CategoricalOption>();
        }

        public void RemoveAnswer(Identity questionIdentity)
        {
        }
    }
}