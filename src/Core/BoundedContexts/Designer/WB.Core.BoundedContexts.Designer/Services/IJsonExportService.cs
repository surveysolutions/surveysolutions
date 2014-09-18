using System;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IJsonExportService
    {
        TemplateInfo GetQuestionnaireTemplateInfo(Guid templateId);
        TemplateInfo GetQuestionnaireTemplateInfo(QuestionnaireDocument template);
    }
}