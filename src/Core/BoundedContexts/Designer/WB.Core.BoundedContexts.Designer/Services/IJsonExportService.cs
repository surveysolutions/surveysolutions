using System;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IJsonExportService
    {
        TemplateInfo GetQuestionnaireTemplate(Guid templateId);
        TemplateInfo GetQuestionnaireTemplate(QuestionnaireDocument template);
    }
}