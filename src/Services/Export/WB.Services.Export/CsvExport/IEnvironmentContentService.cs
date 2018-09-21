using System.Threading;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport
{
    public interface IEnvironmentContentService
    {
        void CreateEnvironmentFiles(QuestionnaireExportStructure questionnaireExportStructure, string folderPath, CancellationToken cancellationToken);
    }
}
