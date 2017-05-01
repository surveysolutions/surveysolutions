using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.V11;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewExpressionStatePrototypeProvider
    {
        ILatestInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion);

        IInterviewExpressionProcessorV11 GetExpressionProcessor(QuestionnaireIdentity questionnaireIdentity);
    }
}
