using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Tests.Abc;
using WB.UI.Designer.Code.ImportExport;

namespace WB.Tests.Unit.Designer.Applications.ImportExportQuestionnaire
{
    [UseApprovalSubdirectory("QuestionnaireExportServiceTests-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [TestOf(typeof(QuestionnaireExportService))]
    public class QuestionnaireExportServiceTests
    {
        [Test]
        public void when_export_only_questionnaire_should_generate_valid_zip()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithCoverPage(id: Id.g1, coverId: Id.g2, 
                children: new []
                {
                    Create.TextQuestion(Id.g3)
                });
            questionnaireDocument.Title = "New Questionnaire";
            var questionnaireRevision = new QuestionnaireRevision(Id.g1);
            var view = new QuestionnaireView(questionnaireDocument, Enumerable.Empty<SharedPersonView>());
            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(f =>
                f.Load(It.Is<QuestionnaireRevision>(qr => qr == questionnaireRevision)) == view);
            
            var service = CreateQuestionnaireExportService(questionnaireViewFactory);

            using var stream = service.GetBackupQuestionnaire(questionnaireRevision, out var filename);

            ZipArchive zip = new ZipArchive(stream);
            var document = zip.GetEntry("document.json").Open();
            var json = new StreamReader(document, Encoding.UTF8).ReadToEnd();

            Approvals.Verify(json);
        }

        private static QuestionnaireExportService CreateQuestionnaireExportService(
            IQuestionnaireViewFactory questionnaireViewFactory)
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new QuestionnaireAutoMapperProfile());
            }).CreateMapper();

            var service = new QuestionnaireExportService(questionnaireViewFactory,
                Mock.Of<IAttachmentService>(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<ITranslationImportExportService>(),
                Mock.Of<ICategoriesImportExportService>(),
                Mock.Of<ILogger<QuestionnaireExportService>>(),
                new FileSystemIOAccessor(),
                Create.InMemoryDbContext(),
                new ImportExportQuestionnaireMapper(mapper),
                new QuestionnaireSerializer());
            return service;
        }
    }
}