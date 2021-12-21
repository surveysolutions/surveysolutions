using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Applications.ImportExportQuestionnaire
{
    [UseApprovalSubdirectory("CategoriesImportExportService-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [TestOf(typeof(CategoriesImportExportService))]
    public class CategoriesImportExportServiceTests
    {
        [Test]
        public async Task when_get_categories_json_should_generate_correct_json_structure()
        {
            var categories = new List<CategoriesInstance>()
            {
                new CategoriesInstance()
                {
                    Text = "hello", Value = 17, CategoriesId = Id.g1, Id = 1, QuestionnaireId = Id.g7, SortIndex = 1
                },
                new CategoriesInstance()
                {
                    Text = "hi", Value = 18, CategoriesId = Id.g1, Id = 2, QuestionnaireId = Id.g7, SortIndex = 0,
                    ParentId = 17
                },
            };
            
            var dbContext = Create.InMemoryDbContext();
            await dbContext.CategoriesInstances.AddRangeAsync(categories);
            await dbContext.SaveChangesAsync();

            var service = CreateCategoriesService(dbContext);

            var json = service.GetCategoriesJson(Id.g7, Id.g1);
            Approvals.Verify(json);
        }

        [Test]
        public void when_get_categories_from_json_should_generate_correct_models()
        {
            var json = @"[
                {
                    ""Value"": 18,
                    ""ParentValue"": 17,
                    ""Text"": ""hi""
                },
                {
                    ""Value"": 17,
                    ""Text"": ""hello""
                }
            ]";
            
            var dbContext = Create.InMemoryDbContext();

            var service = CreateCategoriesService(dbContext);
            service.StoreCategoriesFromJson(Id.g7, Id.g1, json);
            dbContext.SaveChanges();

            var categoriesFromDb = dbContext.CategoriesInstances.ToList();
            var categories = new List<CategoriesInstance>()
            {
                new CategoriesInstance()
                {
                    Text = "hi", Value = 18, CategoriesId = Id.g1, Id = 1, QuestionnaireId = Id.g7, SortIndex = 0,
                    ParentId = 17
                },
                new CategoriesInstance()
                {
                    Text = "hello", Value = 17, CategoriesId = Id.g1, Id = 2, QuestionnaireId = Id.g7, SortIndex = 1
                },
            };

            categories.Should().BeEquivalentTo(categoriesFromDb);
        }

        private ICategoriesImportExportService CreateCategoriesService(DesignerDbContext dbContext)
        {
            return new CategoriesImportExportService(
                dbContext,
                new QuestionnaireSerializer());
        }
    }
}