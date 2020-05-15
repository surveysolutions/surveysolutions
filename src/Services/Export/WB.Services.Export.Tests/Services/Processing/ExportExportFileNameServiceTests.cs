using System.Threading.Tasks;
using NUnit.Framework;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Tests.Services.Processing
{
    [TestFixture]
    public class ExportExportFileNameServiceTests
    {
        [Test]
        public async Task should_include_translation_name_in_export_file_name()
        {
            var questionnaire = Create.QuestionnaireDocument(
                id:Id.g1,
                version: 45,
                variableName: "var1");
            questionnaire.Translations.Add(new Translation
            {
                Id = Id.gA,
                Name = "Перевод"
            });
            
            var service = Create.ExportExportFileNameService(questionnaireStorage: Create.QuestionnaireStorage(questionnaire));

            var fileNameForExportArchive = await service.GetFileNameForExportArchiveAsync(
                new ExportSettings
            {
                Translation = Id.gA,
                Status = InterviewStatus.Completed,
                ExportFormat = DataExportFormat.Tabular,
                QuestionnaireId = questionnaire.QuestionnaireId
            }, questionnaire.VariableName);

            Assert.That(fileNameForExportArchive, Is.EqualTo("var1_Tabular_Completed_Perevod.zip"));
        }

        [Test]
        public async Task should_not_include_translation_name_when_not_requested()
        {
            var questionnaire = Create.QuestionnaireDocument(
                id:Id.g1,
                version: 45,
                variableName: "var1");
            
            var service = Create.ExportExportFileNameService(questionnaireStorage: Create.QuestionnaireStorage(questionnaire));

            var fileNameForExportArchive = await service.GetFileNameForExportArchiveAsync(
                new ExportSettings
                {
                    Status = InterviewStatus.Completed,
                    ExportFormat = DataExportFormat.Tabular,
                    QuestionnaireId = questionnaire.QuestionnaireId
                }, questionnaire.VariableName);

            Assert.That(fileNameForExportArchive, Is.EqualTo("var1_Tabular_Completed.zip"));
            
        }
    }
}
