using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V11
{
    public interface IInterviewExpressionProcessorV11
    {
        void RemoveRoster(Identity rosterId);

        void RemoveAnswer(Identity questionIdentity);

        void SetInterviewProperties(IInterviewProperties properties);
        
        IEnumerable<CategoricalOption> FilterOptionsForQuestion(Identity questionIdentity, IEnumerable<CategoricalOption> options);
    }
}