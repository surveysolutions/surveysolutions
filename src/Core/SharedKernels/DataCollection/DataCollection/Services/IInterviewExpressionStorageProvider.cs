using System;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewExpressionStorageProvider
    {
        IInterviewExpressionStorage GetExpressionStorage(QuestionnaireIdentity questionnaireIdentity);
        Type GetExpressionStorageType(QuestionnaireIdentity questionnaireIdentity);
    }
}
