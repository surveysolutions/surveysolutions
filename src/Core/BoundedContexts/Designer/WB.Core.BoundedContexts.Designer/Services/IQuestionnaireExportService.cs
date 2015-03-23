using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireExportService
    {
        TemplateInfo GetQuestionnaireTemplateInfo(QuestionnaireDocument template);
    }
}