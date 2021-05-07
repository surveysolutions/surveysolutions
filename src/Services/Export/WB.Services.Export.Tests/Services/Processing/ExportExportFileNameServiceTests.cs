using System.Threading.Tasks;
using NUnit.Framework;
using WB.Services.Export.Infrastructure.Implementation;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

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
            
            var service = Create.ExportExportFileNameService(questionnaireStorage: Create.QuestionnaireStorage(questionnaire),
                fileSystemAccessor: new FileSystemAccessor());

            var fileNameForExportArchive = await service.GetFileNameForExportArchiveAsync(
                new ExportSettings(
                    tenant:new TenantInfo("http://test",""), 
                    translation : Id.gA,
                    status : InterviewStatus.Completed,
                    exportFormat : DataExportFormat.Tabular,
                    questionnaireId : questionnaire.QuestionnaireId
            ), questionnaire.VariableName);

            Assert.That(fileNameForExportArchive, Is.EqualTo("var1_Tabular_Completed_Perevod.zip"));
        }

        [Test]
        public async Task should_not_include_translation_name_in_export_file_name_if_no_translation_exists()
        {
            var questionnaire = Create.QuestionnaireDocument(
                id: Id.g1,
                version: 45,
                variableName: "var1");
            questionnaire.Translations.Add(new Translation
            {
                Id = Id.gA,
                Name = "Перевод"
            });

            var service = Create.ExportExportFileNameService(questionnaireStorage: Create.QuestionnaireStorage(questionnaire),
                fileSystemAccessor: new FileSystemAccessor());

            var fileNameForExportArchive = await service.GetFileNameForExportArchiveAsync(
                new ExportSettings(
                    tenant: new TenantInfo("http://test", ""),
                    translation: Id.gF,
                    status: InterviewStatus.Completed,
                    exportFormat: DataExportFormat.Tabular,
                    questionnaireId: questionnaire.QuestionnaireId
                ), questionnaire.VariableName);

            Assert.That(fileNameForExportArchive, Is.EqualTo("var1_Tabular_Completed.zip"));
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
                new ExportSettings(
                    tenant: new TenantInfo("http://test",""), 
                    status : InterviewStatus.Completed,
                    exportFormat : DataExportFormat.Tabular,
                    questionnaireId : questionnaire.QuestionnaireId
                ), questionnaire.VariableName);

            Assert.That(fileNameForExportArchive, Is.EqualTo("var1_Tabular_Completed.zip"));
            
        }

        [Test]
        public async Task should_generate_correct_file_name_if_name_was_not_provided()
        {
            long version = 45;
            var questionnaire = Create.QuestionnaireDocument(
                id: Id.g1,
                version: version,
                variableName: "var1");

            var service = Create.ExportExportFileNameService(questionnaireStorage: Create.QuestionnaireStorage(questionnaire));

            var fileNameForExportArchive = await service.GetFileNameForExportArchiveAsync(
                new ExportSettings(
                    tenant: new TenantInfo("http://test", ""),
                    status: InterviewStatus.Completed,
                    exportFormat: DataExportFormat.Tabular,
                    questionnaireId: questionnaire.QuestionnaireId
                ));

            Assert.That(fileNameForExportArchive, Is.EqualTo($"{questionnaire.QuestionnaireId}_Tabular_Completed.zip"));

        }
    }
}
