using System;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IPreloadingTemplateService
    {
        string GetFilePathToPreloadingTemplate(Guid questionnaireId, long version);
    }
}
