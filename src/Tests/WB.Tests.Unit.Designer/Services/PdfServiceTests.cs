using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.FileSystem;
using WB.Tests.Abc;
using WB.UI.Designer.Areas.Pdf.Services;
using WB.UI.Designer.Areas.Pdf.Utils;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit.Designer.Services
{
    [TestOf(typeof(PdfService))]
    internal class PdfServiceTests
    {
        private Mock<IViewRenderService> mockViewRenderService;
        private Mock<ILogger<PdfService>> mockLogger;
        private Mock<IPdfFactory> mockPdfFactory;
        private Mock<IFileSystemAccessor> mockFileSystemAccessor;
        private Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private Mock<IPdfQuery> mockPdfQuery;
        private Mock<HttpContext> mockHttpContext;
        private Mock<HttpRequest> mockHttpRequest;
        
        private PdfService pdfService;
        private QuestionnaireRevision questionnaireRevision;
        private Guid userId;
        private Guid translationId;

        [SetUp]
        public void Setup()
        {
            this.mockViewRenderService = new Mock<IViewRenderService>();
            this.mockLogger = new Mock<ILogger<PdfService>>();
            this.mockPdfFactory = new Mock<IPdfFactory>();
            this.mockFileSystemAccessor = new Mock<IFileSystemAccessor>();
            this.mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            this.mockPdfQuery = new Mock<IPdfQuery>();
            this.mockHttpContext = new Mock<HttpContext>();
            this.mockHttpRequest = new Mock<HttpRequest>();

            this.userId = Id.g1;
            this.translationId = Id.g2;
            this.questionnaireRevision = new QuestionnaireRevision(Id.g3, Id.g4);

            // Create real ClaimsPrincipal with required claims instead of mocking
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var user = new ClaimsPrincipal(identity);

            // Setup HttpContext
            this.mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(this.mockHttpContext.Object);
            this.mockHttpContext.Setup(x => x.Request).Returns(this.mockHttpRequest.Object);
            this.mockHttpContext.Setup(x => x.User).Returns(user);
            
            // Setup HttpRequest
            this.mockHttpRequest.Setup(x => x.Scheme).Returns("https");
            this.mockHttpRequest.Setup(x => x.Host).Returns(new HostString("example.com"));
            this.mockHttpRequest.Setup(x => x.PathBase).Returns("/app");

            this.pdfService = new PdfService(
                this.mockViewRenderService.Object,
                this.mockLogger.Object,
                this.mockPdfFactory.Object,
                this.mockFileSystemAccessor.Object,
                this.mockHttpContextAccessor.Object,
                this.mockPdfQuery.Object);
        }

        private static PdfQuestionnaireModel CreateTestPdfQuestionnaireModel()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter();
            var settings = new PdfSettings();
            var allItems = new List<IComposite>();
            var created = new PdfQuestionnaireModel.ModificationStatisticsByUser
            {
                UserId = Guid.NewGuid(),
                Name = "Creator",
                Date = DateTime.UtcNow
            };
            var lastModified = new PdfQuestionnaireModel.ModificationStatisticsByUser
            {
                UserId = Guid.NewGuid(),
                Name = "Modifier", 
                Date = DateTime.UtcNow
            };
            var requested = new PdfQuestionnaireModel.ModificationStatisticsByUser
            {
                UserId = Guid.NewGuid(),
                Name = "Requester",
                Date = DateTime.UtcNow
            };

            return new PdfQuestionnaireModel(questionnaire, settings, allItems, created, lastModified, requested);
        }

        [Test]
        public async Task GetHtmlContent_ShouldReturnHtmlBytes_WhenValidInput()
        {
            // Arrange
            var expectedHtml = "<html><body>Test questionnaire</body></html>";
            var questionnaire = CreateTestPdfQuestionnaireModel();
            
            this.mockPdfFactory.Setup(x => x.Load(
                this.questionnaireRevision, 
                this.userId, 
                "testuser", 
                this.translationId, 
                false))
                .Returns(() => questionnaire);

            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(
                It.IsAny<string>(), 
                It.IsAny<object>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<Microsoft.AspNetCore.Routing.RouteData>()))
                .ReturnsAsync(expectedHtml);

            // Act
            var result = await this.pdfService.GetHtmlContent(this.questionnaireRevision, this.translationId);

            // Assert
            Assert.That(result, Is.Not.Null);
            var resultString = Encoding.UTF8.GetString(result);
            Assert.That(resultString, Is.EqualTo(expectedHtml));
        }

        [Test]
        public async Task Enqueue_ShouldRemoveExistingAndCreateNew_WhenExistingProgressExists()
        {
            // Arrange
            var existingProgress = new PdfGenerationProgress();
            var newProgress = new PdfGenerationProgress();
            var questionnaire = CreateTestPdfQuestionnaireModel();
            var key = this.userId + ":" + DocumentType.Pdf + ":" + this.questionnaireRevision + ":" + this.translationId;

            this.mockPdfQuery.Setup(x => x.GetOrNull(key)).Returns(existingProgress);
            this.mockPdfQuery.Setup(x => x.GetOrAdd(this.userId, key, It.IsAny<Func<PdfGenerationProgress, CancellationToken, Task>>()))
                .Returns(newProgress);

            this.mockPdfFactory.Setup(x => x.Load(
                this.questionnaireRevision,
                this.userId,
                "testuser",
                this.translationId,
                false))
                .Returns(() => questionnaire);

            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Microsoft.AspNetCore.Routing.RouteData>()))
                .ReturnsAsync("<html>content</html>");

            // Act
            var result = await this.pdfService.Enqueue(this.questionnaireRevision, this.translationId, DocumentType.Pdf, 120);

            // Assert
            this.mockPdfQuery.Verify(x => x.Remove(key), Times.Once);
            this.mockPdfQuery.Verify(x => x.GetOrAdd(this.userId, key, It.IsAny<Func<PdfGenerationProgress, CancellationToken, Task>>()), Times.Once);
            Assert.That(result, Is.EqualTo(newProgress));
        }

        [Test]
        public async Task Enqueue_ShouldCreateNewProgress_WhenNoExistingProgress()
        {
            // Arrange
            var newProgress = new PdfGenerationProgress();
            var questionnaire = CreateTestPdfQuestionnaireModel();
            var key = this.userId + ":" + DocumentType.Html + ":" + this.questionnaireRevision + ":" + this.translationId;

            this.mockPdfQuery.Setup(x => x.GetOrNull(key)).Returns((PdfGenerationProgress)null);
            this.mockPdfQuery.Setup(x => x.GetOrAdd(this.userId, key, It.IsAny<Func<PdfGenerationProgress, CancellationToken, Task>>()))
                .Returns(newProgress);

            this.mockPdfFactory.Setup(x => x.Load(
                this.questionnaireRevision,
                this.userId,
                "testuser",
                this.translationId,
                false))
                .Returns(() => questionnaire);

            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Microsoft.AspNetCore.Routing.RouteData>()))
                .ReturnsAsync("<html>content</html>");

            // Act
            var result = await this.pdfService.Enqueue(this.questionnaireRevision, this.translationId, DocumentType.Html, null);

            // Assert
            this.mockPdfQuery.Verify(x => x.Remove(key), Times.Never);
            this.mockPdfQuery.Verify(x => x.GetOrAdd(this.userId, key, It.IsAny<Func<PdfGenerationProgress, CancellationToken, Task>>()), Times.Once);
            Assert.That(result, Is.EqualTo(newProgress));
        }

        [Test]
        public void Status_ShouldReturnProgress_WhenProgressExists()
        {
            // Arrange
            var progress = new PdfGenerationProgress();
            var key = this.userId + ":" + DocumentType.Pdf + ":" + this.questionnaireRevision + ":" + this.translationId;
            
            this.mockPdfQuery.Setup(x => x.GetOrNull(key)).Returns(progress);

            // Act
            var result = this.pdfService.Status(this.questionnaireRevision, this.translationId, DocumentType.Pdf);

            // Assert
            Assert.That(result, Is.EqualTo(progress));
        }

        [Test]
        public void Status_ShouldReturnNull_WhenProgressDoesNotExist()
        {
            // Arrange
            var key = this.userId + ":" + DocumentType.Pdf + ":" + this.questionnaireRevision + ":" + this.translationId;
            
            this.mockPdfQuery.Setup(x => x.GetOrNull(key)).Returns((PdfGenerationProgress)null);

            // Act
            var result = this.pdfService.Status(this.questionnaireRevision, this.translationId, DocumentType.Pdf);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Download_ShouldReturnFileContentAndCleanup_WhenProgressIsFinished()
        {
            // Arrange
            var progress = new PdfGenerationProgress();
            progress.Start();
            progress.Finish();
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            var key = this.userId + ":" + DocumentType.Pdf + ":" + this.questionnaireRevision + ":" + this.translationId;

            this.mockPdfQuery.Setup(x => x.GetOrNull(key)).Returns(progress);
            this.mockFileSystemAccessor.Setup(x => x.ReadAllBytes(It.Is<string>(s => s == progress.FilePath), It.IsAny<long?>(), It.IsAny<long?>())).Returns(fileContent);

            // Act
            var result = this.pdfService.Download(this.questionnaireRevision, this.translationId, DocumentType.Pdf);

            // Assert
            Assert.That(result, Is.EqualTo(fileContent));
            this.mockFileSystemAccessor.Verify(x => x.DeleteFile(progress.FilePath), Times.Once);
            this.mockPdfQuery.Verify(x => x.Remove(key), Times.Once);
        }

        [Test]
        public void Download_ShouldReturnNull_WhenProgressIsNotFinished()
        {
            // Arrange
            var progress = new PdfGenerationProgress();
            progress.Start();
            var key = this.userId + ":" + DocumentType.Pdf + ":" + this.questionnaireRevision + ":" + this.translationId;

            this.mockPdfQuery.Setup(x => x.GetOrNull(key)).Returns(progress);

            // Act
            var result = this.pdfService.Download(this.questionnaireRevision, this.translationId, DocumentType.Pdf);

            // Assert
            Assert.That(result, Is.Null);
            this.mockFileSystemAccessor.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never);
            this.mockPdfQuery.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Download_ShouldReturnNull_WhenProgressDoesNotExist()
        {
            // Arrange
            var key = this.userId + ":" + DocumentType.Pdf + ":" + this.questionnaireRevision + ":" + this.translationId;

            this.mockPdfQuery.Setup(x => x.GetOrNull(key)).Returns((PdfGenerationProgress)null);

            // Act
            var result = this.pdfService.Download(this.questionnaireRevision, this.translationId, DocumentType.Pdf);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetCurrentInfoJson_ShouldReturnQueryInfo()
        {
            // Arrange
            var expectedJson = "{\"info\": \"test\"}";
            this.mockPdfQuery.Setup(x => x.GetQueryInfoJson()).Returns(expectedJson);

            // Act
            var result = this.pdfService.GetCurrentInfoJson();

            // Assert
            Assert.That(result, Is.EqualTo(expectedJson));
        }

        [Test]
        public void GetHtmlContent_ShouldThrowException_WhenHttpContextIsNull()
        {
            // Arrange
            this.mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => 
                await this.pdfService.GetHtmlContent(this.questionnaireRevision, this.translationId));
        }

        [Test]
        public void GetHtmlContent_ShouldThrowException_WhenUserIsNull()
        {
            // Arrange
            this.mockHttpContext.Setup(x => x.User).Returns((ClaimsPrincipal)null);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => 
                await this.pdfService.GetHtmlContent(this.questionnaireRevision, this.translationId));
        }

        [Test]
        public async Task GetHtmlContent_ShouldReturnEmptyBytes_WhenQuestionnaireIsNull()
        {
            // Arrange
            this.mockPdfFactory.Setup(x => x.Load(
                this.questionnaireRevision,
                this.userId,
                "testuser",
                this.translationId,
                false))
                .Returns((PdfQuestionnaireModel)null);

            // Act
            var result = await this.pdfService.GetHtmlContent(this.questionnaireRevision, this.translationId);

            // Assert
            Assert.That(result, Is.Not.Null);
            var resultString = Encoding.UTF8.GetString(result);
            Assert.That(resultString, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task Enqueue_ShouldSetTimezoneOffset_WhenTimezoneOffsetProvided()
        {
            // Arrange
            var questionnaire = CreateTestPdfQuestionnaireModel();
            var timezoneOffset = 180;
            var capturedQuestionnaire = (PdfQuestionnaireModel)null;

            this.mockPdfFactory.Setup(x => x.Load(
                this.questionnaireRevision,
                this.userId,
                "testuser",
                this.translationId,
                false))
                .Returns(() => questionnaire);

            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Microsoft.AspNetCore.Routing.RouteData>()))
                .Callback<string, object, string, string, Microsoft.AspNetCore.Routing.RouteData>((view, model, _, _, _) =>
                {
                    if (view == "RenderQuestionnaire")
                        capturedQuestionnaire = model as PdfQuestionnaireModel;
                })
                .ReturnsAsync("<html>content</html>");

            this.mockPdfQuery.Setup(x => x.GetOrAdd(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<PdfGenerationProgress, CancellationToken, Task>>()))
                .Returns(new PdfGenerationProgress());

            // Act
            await this.pdfService.Enqueue(this.questionnaireRevision, this.translationId, DocumentType.Pdf, timezoneOffset);

            // Assert
            Assert.That(capturedQuestionnaire, Is.Not.Null);
            Assert.That(capturedQuestionnaire.TimezoneOffsetMinutes, Is.EqualTo(timezoneOffset));
        }

        [Test]
        public async Task Enqueue_ShouldSetZeroTimezoneOffset_WhenTimezoneOffsetIsNull()
        {
            // Arrange
            var questionnaire = CreateTestPdfQuestionnaireModel();
            var capturedQuestionnaire = (PdfQuestionnaireModel)null;

            this.mockPdfFactory.Setup(x => x.Load(
                this.questionnaireRevision,
                this.userId,
                "testuser",
                this.translationId,
                false))
                .Returns(() => questionnaire);

            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Microsoft.AspNetCore.Routing.RouteData>()))
                .Callback<string, object, string, string, Microsoft.AspNetCore.Routing.RouteData>((view, model, _, _, _) =>
                {
                    if (view == "RenderQuestionnaire")
                        capturedQuestionnaire = model as PdfQuestionnaireModel;
                })
                .ReturnsAsync("<html>content</html>");

            this.mockPdfQuery.Setup(x => x.GetOrAdd(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<PdfGenerationProgress, CancellationToken, Task>>()))
                .Returns(new PdfGenerationProgress());

            // Act
            await this.pdfService.Enqueue(this.questionnaireRevision, this.translationId, DocumentType.Pdf, null);

            // Assert
            Assert.That(capturedQuestionnaire, Is.Not.Null);
            Assert.That(capturedQuestionnaire.TimezoneOffsetMinutes, Is.EqualTo(0));
        }
    }
}
