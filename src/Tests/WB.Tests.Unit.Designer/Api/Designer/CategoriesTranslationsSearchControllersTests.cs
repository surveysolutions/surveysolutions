#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.Tests.Unit.Designer.Api.Designer
{
    [TestFixture]
    [TestOf(typeof(CategoriesController))]
    public class CategoriesControllerTests
    {
        private Mock<IReusableCategoriesService> service = null!;
        private Mock<IFileSystemAccessor> files = null!;
        private CategoriesController controller = null!;

        [SetUp]
        public void SetUp()
        {
            service = new Mock<IReusableCategoriesService>();
            files = new Mock<IFileSystemAccessor>();
            files.Setup(x => x.MakeValidFileName(It.IsAny<string>())).Returns<string>(x => x);
            controller = new CategoriesController(service.Object, files.Object);
        }

        [Test]
        public void when_template_missing_should_return_not_found()
        {
            service.Setup(x => x.GetTemplate(CategoriesFileType.Excel)).Returns((byte[])null!);

            controller.Get().Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_excel_template_exists_should_return_xlsx_file()
        {
            service.Setup(x => x.GetTemplate(CategoriesFileType.Excel)).Returns(new byte[] { 1 });

            var file = controller.Get().Should().BeOfType<FileContentResult>().Subject;

            file.FileDownloadName.Should().EndWith(".xlsx");
            file.FileContents.Should().Equal(1);
        }

        [Test]
        public void when_tsv_template_exists_should_return_txt_file()
        {
            service.Setup(x => x.GetTemplate(CategoriesFileType.Tsv)).Returns(new byte[] { 2 });

            var file = controller.GetCsv().Should().BeOfType<FileContentResult>().Subject;

            file.ContentType.Should().Be("text/plain");
            file.FileDownloadName.Should().EndWith(".txt");
        }

        [Test]
        public void when_tsv_template_missing_should_return_not_found()
        {
            service.Setup(x => x.GetTemplate(CategoriesFileType.Tsv)).Returns((byte[])null!);

            controller.GetCsv().Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_categories_file_has_no_content_should_return_not_found()
        {
            service.Setup(x => x.GetAsFile(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>(), CategoriesFileType.Excel, false))
                .Returns(new CategoriesFile { Content = null });

            controller.Get(new QuestionnaireRevision(Guid.NewGuid()), Guid.NewGuid()).Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_categories_file_is_missing_should_return_not_found()
        {
            service.Setup(x => x.GetAsFile(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>(), CategoriesFileType.Excel, false))
                .Returns((CategoriesFile?)null);

            controller.Get(new QuestionnaireRevision(Guid.NewGuid()), Guid.NewGuid()).Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_categories_file_has_name_should_use_it_in_file_name()
        {
            service.Setup(x => x.GetAsFile(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>(), CategoriesFileType.Excel, false))
                .Returns(new CategoriesFile { Content = new byte[] { 1 }, CategoriesName = "cities", QuestionnaireTitle = "Q" });

            var file = controller.Get(new QuestionnaireRevision(Guid.NewGuid()), Guid.NewGuid())
                .Should().BeOfType<FileContentResult>().Subject;

            file.FileDownloadName.Should().Be("[cities]Q.xlsx");
        }

        [Test]
        public void when_categories_name_is_empty_should_use_default_name()
        {
            service.Setup(x => x.GetAsFile(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>(), CategoriesFileType.Excel, false))
                .Returns(new CategoriesFile { Content = new byte[] { 1 }, CategoriesName = "", QuestionnaireTitle = "Q" });

            var file = controller.Get(new QuestionnaireRevision(Guid.NewGuid()), Guid.NewGuid())
                .Should().BeOfType<FileContentResult>().Subject;

            file.FileDownloadName.Should().Be("[New categories]Q.xlsx");
        }
    }

    [TestFixture]
    [TestOf(typeof(TranslationsController))]
    public class TranslationsControllerTests
    {
        private Mock<IDesignerTranslationService> service = null!;
        private TranslationsController controller = null!;

        [SetUp]
        public void SetUp()
        {
            service = new Mock<IDesignerTranslationService>();
            var files = new Mock<IFileSystemAccessor>();
            files.Setup(x => x.MakeValidFileName(It.IsAny<string>())).Returns<string>(x => x);
            controller = new TranslationsController(service.Object, files.Object);
        }

        [Test]
        public void when_template_has_no_content_should_return_not_found()
        {
            service.Setup(x => x.GetTemplateAsExcelFile(It.IsAny<QuestionnaireRevision>()))
                .Returns(new TranslationFile("Q", null!, ""));

            controller.Get(new QuestionnaireRevision(Guid.NewGuid())).Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_template_exists_should_return_xlsx_with_default_translation_name()
        {
            service.Setup(x => x.GetTemplateAsExcelFile(It.IsAny<QuestionnaireRevision>()))
                .Returns(new TranslationFile("Q", new byte[] { 1 }, ""));

            var file = controller.Get(new QuestionnaireRevision(Guid.NewGuid()))
                .Should().BeOfType<FileContentResult>().Subject;

            file.FileDownloadName.Should().Be("{New translation}Q.xlsx");
        }

        [Test]
        public void when_translation_exists_should_return_named_xlsx()
        {
            var id = Guid.NewGuid();
            service.Setup(x => x.GetAsExcelFile(It.IsAny<QuestionnaireRevision>(), id))
                .Returns(new TranslationFile("Q", new byte[] { 1 }, "French"));

            var file = controller.Get(new QuestionnaireRevision(Guid.NewGuid()), id)
                .Should().BeOfType<FileContentResult>().Subject;

            file.FileDownloadName.Should().Be("{French}Q.xlsx");
            file.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [Test]
        public void when_translation_has_no_content_should_return_not_found()
        {
            service.Setup(x => x.GetAsExcelFile(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>()))
                .Returns(new TranslationFile("Q", null!, "French"));

            controller.Get(new QuestionnaireRevision(Guid.NewGuid()), Guid.NewGuid()).Should().BeOfType<NotFoundResult>();
        }
    }

    [TestFixture]
    [TestOf(typeof(SearchController))]
    public class SearchControllerTests
    {
        [Test]
        public async Task when_requesting_folders_should_return_all_folders()
        {
            var folders = new List<QuestionnaireListViewFolder> { new QuestionnaireListViewFolder(Guid.NewGuid(), "F") };
            var storage = new Mock<IPublicFoldersStorage>();
            storage.Setup(x => x.GetAllFoldersAsync()).ReturnsAsync(folders);
            var controller = new SearchController(storage.Object, Mock.Of<IQuestionnaireSearchStorage>());

            (await controller.GetFolders()).Should().BeSameAs(folders);
        }

        [Test]
        public void when_searching_should_pass_query_and_map_result()
        {
            var questionnaireId = Guid.NewGuid();
            var entityId = Guid.NewGuid();
            var sectionId = Guid.NewGuid();
            var folderId = Guid.NewGuid();
            SearchInput? captured = null;
            var search = new Mock<IQuestionnaireSearchStorage>();
            search.Setup(x => x.Search(It.IsAny<SearchInput>()))
                .Callback<SearchInput>(i => captured = i)
                .Returns(new SearchResult
                {
                    TotalCount = 7,
                    Items = new List<SearchResultEntity>
                    {
                        new SearchResultEntity
                        {
                            QuestionnaireId = questionnaireId, EntityId = entityId, SectionId = sectionId,
                            Title = "t", QuestionnaireTitle = "qt", EntityType = "Question",
                            FolderId = folderId, FolderName = "fn"
                        },
                        new SearchResultEntity { QuestionnaireId = questionnaireId, EntityId = entityId }
                    }
                });
            var controller = new SearchController(Mock.Of<IPublicFoldersStorage>(), search.Object);

            var ok = controller.Search(new SearchQueryModel { Query = "abc", PageIndex = 2, PageSize = 5, FolderId = folderId })
                .Should().BeOfType<OkObjectResult>().Subject;

            captured!.Query.Should().Be("abc");
            captured.PageIndex.Should().Be(2);
            captured.PageSize.Should().Be(5);
            captured.FolderId.Should().Be(folderId);

            var model = ok.Value.Should().BeOfType<SearchResultModel>().Subject;
            model.Total.Should().Be(7);
            model.Entities.Should().HaveCount(2);
            model.Entities[0].QuestionnaireId.Should().Be(questionnaireId.ToString("N"));
            model.Entities[0].ItemId.Should().Be(entityId.ToString("N"));
            model.Entities[0].SectionId.Should().Be(sectionId.ToString("N"));
            model.Entities[0].ItemType.Should().Be("Question");
            model.Entities[0].Folder!.PublicId.Should().Be(folderId);
            model.Entities[0].Folder!.Title.Should().Be("fn");
            model.Entities[1].Folder.Should().BeNull();
        }

        [Test]
        public void search_query_model_should_have_sensible_defaults()
        {
            var model = new SearchQueryModel();

            model.PageSize.Should().Be(20);
            model.PageIndex.Should().Be(0);
            model.PrivateOnly.Should().BeFalse();
        }
    }
}

