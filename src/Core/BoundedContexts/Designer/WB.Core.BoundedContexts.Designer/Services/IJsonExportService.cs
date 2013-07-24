using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IJsonExportService
    {
        string GetQuestionnaireTemplate(Guid templateId);
    }
}