using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using AutoMapper;
using Main.Core.Documents;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Validation;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Infrastructure.Native.Storage;
using WB.UI.Designer.Code.ImportExport;

namespace WB.Tests.Unit.Designer.Applications.ImportExportQuestionnaire
{
    [TestFixture]
    [UseApprovalSubdirectory("SchemaTests-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    public class SchemaTests
    {
        [Test]
        public void when_generate_schema_should_be_equal_to_pattern_file()
        {
            JsonSchemaGeneratorSettings settings = new JsonSchemaGeneratorSettings()
            {
                FlattenInheritanceHierarchy = false,
            };
            settings.DefaultEnumHandling = EnumHandling.String;
            var jsonSchema = JsonSchema.FromType<Questionnaire>(settings);
            var json = jsonSchema.ToJson();
            
            Approvals.Verify(json);
        }

        [Ignore("Temp")]
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
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new QuestionnaireAutoMapperProfile());
            }).CreateMapper();

            var importExportQuestionnaireService = new ImportExportQuestionnaireMapper(mapper);
            var questionnaire = importExportQuestionnaireService.Map(questionnaireDocument);
            var json = new QuestionnaireSerializer().Serialize(questionnaire);

            var testType = typeof(SchemaTests);
            var readResourceFile = $"{testType.Namespace}.SchemaExample.json";

            await using Stream stream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            using StreamReader reader = new StreamReader(stream);
            string schemaText = await reader.ReadToEndAsync();

            var jsonSchema = await JsonSchema.FromJsonAsync(schemaText);

            var validationErrors = jsonSchema.Validate(json);
            return validationErrors;
        }
    }
}