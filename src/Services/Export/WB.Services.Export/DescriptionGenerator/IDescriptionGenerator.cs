using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.DescriptionGenerator
{
    public interface IDescriptionGenerator
    {
        void GenerateDescriptionFile(QuestionnaireExportStructure questionnaire, string basePath, string dataFilesExtension);
    }
}
