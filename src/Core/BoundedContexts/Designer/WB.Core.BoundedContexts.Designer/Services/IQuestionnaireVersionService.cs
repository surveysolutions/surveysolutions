using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireVersionService
    {
        Version GetLatestSupportedVersion();

        bool IsClientVersionSupported(Version clientVersion);
    }
}