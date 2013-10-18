using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IJsonExportService
    {
        TemplateInfo GetQuestionnaireTemplate(Guid templateId);
    }
}