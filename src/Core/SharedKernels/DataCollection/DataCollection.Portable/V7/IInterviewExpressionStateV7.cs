using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V6;

namespace WB.Core.SharedKernels.DataCollection.V7
{
    public interface IInterviewExpressionStateV7: IInterviewExpressionStateV6
    {
        LinkedQuestionOptionsChanges ProcessLinkedQuestionFilters();
        void EnableLinkedQuestionOptions(Identity[] options);
        void DisableLinkedQuestionOptions(Identity[] options);
        new IInterviewExpressionStateV7 Clone();
    }
}