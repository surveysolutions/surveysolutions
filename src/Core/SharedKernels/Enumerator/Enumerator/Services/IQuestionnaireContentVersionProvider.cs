using System;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IQuestionnaireContentVersionProvider
    {
        Version GetSupportedQuestionnaireContentVersion();
    }
}
