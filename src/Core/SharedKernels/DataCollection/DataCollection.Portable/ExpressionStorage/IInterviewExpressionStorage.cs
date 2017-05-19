using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage
{
    public interface IInterviewExpressionStorage
    {
        void Initialize(IInterviewState state, IInterviewProperties properties);

        // move to questionnaire
        List<Guid> GetExpressionsOrder();
        
        IInterviewLevel GetLevel(Identity rosterIdentity);
    }
}