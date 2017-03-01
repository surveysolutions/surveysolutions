using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V11
{
    public interface IInterviewExpressionProcessorV11
    {
        void Init(IInterviewStateAccessorV11 state);

        void RemoveRoster(Identity rosterId);

        void SetInterviewProperties(IInterviewProperties properties);

        void UpdateVariableValue(Identity variableId, object value);

        IEnumerable<CategoricalOption> FilterOptionsForQuestion(Identity questionIdentity, IEnumerable<CategoricalOption> options);
        void RemoveAnswer(Identity questionIdentity);
    }
}