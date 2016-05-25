using System;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ISupportedVersionProvider
    {
        Version GetSupportedQuestionnaireVersion();
    }
}
