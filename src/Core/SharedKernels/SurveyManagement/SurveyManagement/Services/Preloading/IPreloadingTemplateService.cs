using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    public interface IPreloadingTemplateService
    {
        string GetFilePathToPreloadingTemplate(Guid questionnaireId, long version);
    }
}
