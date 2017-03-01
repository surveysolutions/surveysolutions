using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.V11
{
    public abstract class AbstractInterviewExpressionProcessorV11<T> : IInterviewExpressionProcessorV11
        where T : IInterviewLevelV11
    {
        public void Init(IInterviewStateAccessorV11 state)
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