using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer;

[TestOf(typeof(ReusableCategoriesService))]
public class CategoriesServiceTests
{
    [Test]
    public void when_generate_categories_file_should_save_options_order()
    {
        var document = Create.QuestionnaireDocument(Id.g1);
        document.Categories.Add(Create.Categories(Id.g2));
        var view = new QuestionnaireView(document, Array.Empty<SharedPersonView>());
        
        var revision = new QuestionnaireRevision(Id.g1);
        
        var documentStorage = Mock.Of<IQuestionnaireViewFactory>(s =>
            s.Load(revision) == view);

        var categoriesDb = Create.InMemoryDbContext();
        categoriesDb.CategoriesInstances.AddRange(
            new[]
            {
                Create.CategoriesInstance(Id.g1, Id.g2, 1, 1, "1"),
                Create.CategoriesInstance(Id.g1, Id.g2, 3, 2, "3"),
                Create.CategoriesInstance(Id.g1, Id.g2, 2, 3, "2"),
            });
        categoriesDb.SaveChanges();

        Mock<ICategoriesExportService> categoriesExportService = new Mock<ICategoriesExportService>();
        
        var service = CreateCategoriesService(documentStorage, categoriesDb);

        var excelFile = service.GetAsFile(revision, Id.g2, CategoriesFileType.Excel);
        
        categoriesExportService.Verify(m => 
            m.GetAsExcelFile(It.Is<IEnumerable<CategoriesItem>>(list =>
                list.First().Id == 1 && list.First().ParentId == null && list.First().Text == "1"
                && list.Second().Id == 3 && list.Second().ParentId == null && list.Second().Text == "3"
                && list.Last().Id == 2 && list.Last().ParentId == null && list.Last().Text == "2"
            )), Times.Once);
    }

    private IReusableCategoriesService CreateCategoriesService(IQuestionnaireViewFactory documentStorage,
        DesignerDbContext designerDbContext)
    {
        return new ReusableCategoriesService(
            designerDbContext ?? Mock.Of<DesignerDbContext>(),
            documentStorage ?? Mock.Of<IQuestionnaireViewFactory>(),
            Create.CategoriesExtractFactory());
    }
}
