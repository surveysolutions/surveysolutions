using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IExportService
    {
        string GetQuestionnaireTemplate(Guid templateId);
    }
}