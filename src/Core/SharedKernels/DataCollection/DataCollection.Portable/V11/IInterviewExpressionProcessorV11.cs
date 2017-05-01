using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V11
{
    public interface IInterviewExpressionProcessorV11
    {
        void Initialize(IInterviewState state, IInterviewProperties properties);

        // move to questionnaire
        List<Guid> GetExpressionsOrder();
        
        IInterviewLevelV11 GetLevel(Identity rosterIdentity);
        //IEnumerable<CategoricalOption> FilterOptionsForQuestion(Identity questionIdentity, IEnumerable<CategoricalOption> options);
    }
}