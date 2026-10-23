using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Main.Core.Documents;
using NJsonSchema;
using NJsonSchema.Validation;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Infrastructure.Native.Storage;
using WB.UI.Designer.Code.ImportExport;

namespace WB.Tests.Unit.Designer.Applications.ImportExportQuestionnaire
{
    [TestFixture]
    public class QuestionnaireValidateBySchema
    {
        [Test]
        public async Task when_validate_questionnaire_should_be_valid_by_schema()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                Create.TextQuestion()
            );

            var validationErrors = await Validate(questionnaireDocument);

            Assert.That(validationErrors.Count, Is.EqualTo(0));
        }

        private static async Task<ICollection<ValidationError>> Validate(QuestionnaireDocument questionnaireDocument)
        {
            var importExportQuestionnaireService = new ImportExportQuestionnaireMapper();
            var questionnaire = importExportQuestionnaireService.Map(questionnaireDocument);
            var json = new QuestionnaireSerializer().Serialize(questionnaire);

            var testType = typeof(QuestionnaireImportService);
            var readResourceFile = $"{testType.Namespace}.QuestionnaireSchema.json";

            await using Stream stream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            using StreamReader reader = new StreamReader(stream);
            string schemaText = await reader.ReadToEndAsync();

            var schema = await JsonSchema.FromJsonAsync(schemaText);

            return schema.Validate(json);
        }
    }
}
