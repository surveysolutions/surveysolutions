using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using AutoMapper;
using Main.Core.Documents;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Infrastructure.Native.Storage;
using WB.UI.Designer.Code.ImportExport;

namespace WB.Tests.Unit.Designer.Applications.ImportExportQuestionnaire
{
    [TestFixture]
    [UseApprovalSubdirectory("QuestionnaireValidateBySchema-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    public class QuestionnaireValidateBySchema
    {
        [Test]
        public void when_generate_schema_should_be_equal_to_pattern_file()
        {
            JSchemaGenerator generator = new JSchemaGenerator();
            generator.SchemaReferenceHandling = SchemaReferenceHandling.All;
            generator.GenerationProviders.Add(new StringEnumGenerationProvider());
            generator.GenerationProviders.Add(new MyJSchemaGenerationProvider());
            
            JSchema jsonSchema = generator.Generate(typeof(Questionnaire));
            var json = jsonSchema.ToString();
            
            Approvals.Verify(json);
        }
        
        internal class MyJSchemaGenerationProvider : JSchemaGenerationProvider
        {
            public override JSchema GetSchema(JSchemaTypeGenerationContext context)
            {
                var schema = new JSchema();
                var descendants = typeof(IQuestionnaireEntity).Assembly.GetTypes().Where(item => !item.IsAbstract && typeof(IQuestionnaireEntity).IsAssignableFrom(item)).ToList();
                foreach (var descendant in descendants)
                {
                    // The line below never exits, because it's calling MySchemaGenerator.GetSchema again with the same parameter
                    var descendantSchema = context.Generator.Generate(descendant);
                    schema.PatternProperties.Add(descendant.Name + ".*", descendantSchema);
                }
                schema.AllowAdditionalProperties = false;
                schema.Type = JSchemaType.Object;
                return schema;
            }

            public override bool CanGenerateSchema(JSchemaTypeGenerationContext context)
            {
                return context.ObjectType == typeof(IQuestionnaireEntity);
            }
        }


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

            var testType = typeof(ImportExportQuestionnaireMapper);
            var readResourceFile = $"{testType.Namespace}.QuestionnaireSchema.json";

            await using Stream stream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            using StreamReader reader = new StreamReader(stream);
            string schemaText = await reader.ReadToEndAsync();

            JSchema schema = JSchema.Parse(schemaText);

            JToken jToken = JToken.Parse(json);
            jToken.IsValid(schema, out IList<ValidationError> errors);
            return errors;
        }
    }
}