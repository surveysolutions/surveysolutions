using System;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IPreloadingTemplateService
    {
        byte[] GetPrefilledPreloadingTemplateFile(Guid questionnaireId, long version);
        string GetFilePathToPreloadingTemplate(Guid questionnaireId, long version);
    }
}
