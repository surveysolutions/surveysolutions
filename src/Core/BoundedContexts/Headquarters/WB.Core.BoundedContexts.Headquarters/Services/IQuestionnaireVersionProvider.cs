using System;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IQuestionnaireVersionProvider
    {
        long GetNextVersion(Guid questionnaireId);
        long? GetPreviousVersion(Guid questionnaireId, long version);
    }
}