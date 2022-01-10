using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using AutoMapper;
using FluentAssertions;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Applications.ImportExportQuestionnaire
{
    [UseApprovalSubdirectory("TranslationImportExportService-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [TestOf(typeof(TranslationImportExportService))]
    public class TranslationImportExportServiceTests
    {
        [Test]
        public async Task when_get_translations_json_should_generate_correct_json_structure()
        {
            var questionnaire = Create.QuestionnaireDocument(Id.g1, new IComposite[]
            {
                Create.TextQuestion(Id.g4, variable: "text #1"),
                Create.SingleQuestion(Id.g5, variable: "single #2"),
            });
            
            var translations = new List<TranslationInstance>()
            {
                new TranslationInstance()
                {
                    Value = "hello", Type = TranslationType.Title, QuestionnaireId = Id.g1, Id = Id.g2, TranslationId = Id.g7, TranslationIndex = "index1", QuestionnaireEntityId = Id.g4
                },
                new TranslationInstance()
                {
                    Value = "hi", Type = TranslationType.OptionTitle, QuestionnaireId = Id.g1, Id = Id.g3, TranslationId = Id.g7, TranslationIndex = "index0", QuestionnaireEntityId = Id.g5,
                },
            };
            
            var dbContext = Create.InMemoryDbContext();
            await dbContext.TranslationInstances.AddRangeAsync(translations);
            await dbContext.SaveChangesAsync();

            var service = CreateTranslationsService(dbContext);

            var json = service.GetTranslationsJson(questionnaire, Id.g7);
            Approvals.Verify(json);
        }

        [Test]
        public void when_get_translations_from_json_should_generate_correct_models()
        {
            var questionnaire = Create.QuestionnaireDocument(Id.g1, new IComposite[]
            {
                Create.TextQuestion(Id.g4, variable: "text #1"),
                Create.SingleQuestion(Id.g5, variable: "single #2"),
            });

            var json = @"[
                {
                    ""Type"": ""Title"",
                    ""EntityId"": ""44444444-4444-4444-4444-444444444444"",
                    ""EntityVariableName"": ""text #1"",
                    ""OptionIndex"": ""index1"",
                    ""Value"": ""hello""
                },
                {
                    ""Type"": ""OptionTitle"",
                    ""EntityId"": ""55555555-5555-5555-5555-555555555555"",
                    ""EntityVariableName"": ""single #2"",
                    ""OptionIndex"": ""index0"",
                    ""Value"": ""hi""
                }
            ]";
            
            var dbContext = Create.InMemoryDbContext();

            var service = CreateTranslationsService(dbContext);
            service.StoreTranslationsFromJson(questionnaire, Id.g7, json);
            dbContext.SaveChanges();

            var translationsFromDb = dbContext.TranslationInstances.ToList();
            var translations = new List<TranslationInstance>()
            {
                new TranslationInstance()
                {
                    Value = "hi", Type = TranslationType.OptionTitle, QuestionnaireId = Id.g1, Id = Id.g3, TranslationId = Id.g7, TranslationIndex = "index0", QuestionnaireEntityId = Id.g5,
                },
                new TranslationInstance()
                {
                    Value = "hello", Type = TranslationType.Title, QuestionnaireId = Id.g1, Id = Id.g2, TranslationId = Id.g7, TranslationIndex = "index1", QuestionnaireEntityId = Id.g4
                },
            };

            translations.Should().BeEquivalentTo(translationsFromDb, opt => opt.Excluding(q => q.Id));
        }

        private ITranslationImportExportService CreateTranslationsService(DesignerDbContext dbContext)
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new QuestionnaireAutoMapperProfile());
            }).CreateMapper();

            return new TranslationImportExportService(
                dbContext,
                mapper,
                new QuestionnaireSerializer());
        }
    }

}