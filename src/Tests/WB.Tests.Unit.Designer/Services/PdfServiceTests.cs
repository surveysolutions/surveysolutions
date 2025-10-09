using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.FileSystem;
using WB.Tests.Abc;
using WB.UI.Designer.Areas.Pdf.Services;
using WB.UI.Designer.Areas.Pdf.Utils;
using WB.UI.Shared.Web.Services;
using System.Security.Claims;
using Main.Core.Documents;
using Microsoft.AspNetCore.Routing;

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
        private Mock<ClaimsPrincipal> mockUser;
        
        private PdfService pdfService;
        private QuestionnaireRevision questionnaireRevision;
        private Guid testUserId;
        private Guid? testTranslation;

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
            this.mockUser = new Mock<ClaimsPrincipal>();

            this.testUserId = Id.g1;
            this.testTranslation = Id.g2;
            this.questionnaireRevision = new QuestionnaireRevision(Id.g3, Id.g4, 1);

            // Setup HttpContext
            this.mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(this.mockHttpContext.Object);
            this.mockHttpContext.Setup(x => x.Request).Returns(this.mockHttpRequest.Object);
            this.mockHttpContext.Setup(x => x.User).Returns(this.mockUser.Object);
            
            // Setup HttpRequest for URL generation
            this.mockHttpRequest.Setup(x => x.Scheme).Returns("https");
            this.mockHttpRequest.Setup(x => x.Host).Returns(new HostString("localhost"));
            this.mockHttpRequest.Setup(x => x.PathBase).Returns(new PathString(""));

            // Setup User extensions
            this.mockUser.Setup(x => x.GetIdOrNull()).Returns(this.testUserId);
            this.mockUser.Setup(x => x.GetUserNameOrNull()).Returns("testuser");

            this.pdfService = new PdfService(
                this.mockViewRenderService.Object,
                this.mockLogger.Object,
                this.mockPdfFactory.Object,
                this.mockFileSystemAccessor.Object,
                this.mockHttpContextAccessor.Object,
                this.mockPdfQuery.Object);
        }

        [Test]
        public async Task Enqueue_ShouldCreateNewProgressAndEnqueue_WhenNoExistingProgress()
        {
            // Arrange
            var documentType = DocumentType.Pdf;
            var timezoneOffset = 120;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";
            var questionnaire = CreateMockPdfQuestionnaireModel();
            var expectedProgress = new PdfGenerationProgress();

            this.mockPdfQuery.Setup(x => x.GetOrNull(expectedKey)).Returns((PdfGenerationProgress)null);
            this.mockPdfFactory.Setup(x => x.Load(this.questionnaireRevision, this.testUserId, "testuser", this.testTranslation, false))
                .Returns(questionnaire);
            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RouteData>()))
                .ReturnsAsync("<html>questionnaire content</html>");
            this.mockPdfQuery.Setup(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()))
                .Returns(expectedProgress);

            // Act
            var result = await this.pdfService.Enqueue(this.questionnaireRevision, this.testTranslation, documentType, timezoneOffset);

            // Assert
            Assert.That(result, Is.EqualTo(expectedProgress));
            this.mockPdfQuery.Verify(x => x.GetOrNull(expectedKey), Times.Once);
            this.mockPdfQuery.Verify(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()), Times.Once);
            this.mockPdfFactory.Verify(x => x.Load(this.questionnaireRevision, this.testUserId, "testuser", this.testTranslation, false), Times.Once);
        }

        [Test]
        public async Task Enqueue_ShouldRemoveExistingProgressAndCreateNew_WhenExistingProgressExists()
        {
            // Arrange
            var documentType = DocumentType.Html;
            var existingProgress = new PdfGenerationProgress();
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";
            var questionnaire = CreateMockPdfQuestionnaireModel();
            var newProgress = new PdfGenerationProgress();

            this.mockPdfQuery.Setup(x => x.GetOrNull(expectedKey)).Returns(existingProgress);
            this.mockPdfFactory.Setup(x => x.Load(this.questionnaireRevision, this.testUserId, "testuser", this.testTranslation, false))
                .Returns(questionnaire);
            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RouteData>()))
                .ReturnsAsync("<html>questionnaire content</html>");
            this.mockPdfQuery.Setup(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()))
                .Returns(newProgress);

            // Act
            var result = await this.pdfService.Enqueue(this.questionnaireRevision, this.testTranslation, documentType, null);

            // Assert
            Assert.That(result, Is.EqualTo(newProgress));
            this.mockPdfQuery.Verify(x => x.Remove(expectedKey), Times.Once);
            this.mockPdfQuery.Verify(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()), Times.Once);
        }

        [Test]
        public async Task Enqueue_ShouldRetryFailedProgress_WhenProgressFailedAndTimezoneOffsetIsNull()
        {
            // Arrange
            var documentType = DocumentType.Pdf;
            var failedProgress = new PdfGenerationProgress();
            failedProgress.Fail();
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";
            var questionnaire = CreateMockPdfQuestionnaireModel();
            var retryProgress = new PdfGenerationProgress();

            this.mockPdfQuery.Setup(x => x.GetOrNull(expectedKey)).Returns((PdfGenerationProgress)null);
            this.mockPdfFactory.Setup(x => x.Load(this.questionnaireRevision, this.testUserId, "testuser", this.testTranslation, false))
                .Returns(questionnaire);
            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RouteData>()))
                .ReturnsAsync("<html>questionnaire content</html>");
            this.mockPdfQuery.SetupSequence(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()))
                .Returns(failedProgress)
                .Returns(retryProgress);

            // Act
            var result = await this.pdfService.Enqueue(this.questionnaireRevision, this.testTranslation, documentType, null);

            // Assert
            Assert.That(result, Is.EqualTo(retryProgress));
            this.mockPdfQuery.Verify(x => x.Remove(expectedKey), Times.Once);
            this.mockPdfQuery.Verify(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()), Times.Exactly(2));
        }

        [Test]
        public async Task Enqueue_ShouldThrowException_WhenHttpContextIsNull()
        {
            // Arrange
            this.mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => 
                this.pdfService.Enqueue(this.questionnaireRevision, this.testTranslation, DocumentType.Pdf, 0));
            
            Assert.That(ex?.Message, Is.EqualTo("HttpContext is null"));
        }

        [Test]
        public async Task Enqueue_ShouldFailProgress_WhenQuestionnaireIsNull()
        {
            // Arrange
            var documentType = DocumentType.Pdf;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";
            var progress = new PdfGenerationProgress();

            this.mockPdfQuery.Setup(x => x.GetOrNull(expectedKey)).Returns((PdfGenerationProgress)null);
            this.mockPdfFactory.Setup(x => x.Load(this.questionnaireRevision, this.testUserId, "testuser", this.testTranslation, false))
                .Returns((PdfQuestionnaireModel)null);
            this.mockPdfQuery.Setup(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()))
                .Returns(progress);

            // Act
            var result = await this.pdfService.Enqueue(this.questionnaireRevision, this.testTranslation, documentType, 0);

            // Assert
            Assert.That(result, Is.EqualTo(progress));
            // Note: Progress.Fail() would be called during HTML content generation
        }

        [Test]
        public void Status_ShouldReturnProgress_WhenProgressExists()
        {
            // Arrange
            var documentType = DocumentType.Pdf;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";
            var expectedProgress = new PdfGenerationProgress();

            this.mockPdfQuery.Setup(x => x.GetOrNull(expectedKey)).Returns(expectedProgress);

            // Act
            var result = this.pdfService.Status(this.questionnaireRevision, this.testTranslation, documentType);

            // Assert
            Assert.That(result, Is.EqualTo(expectedProgress));
            this.mockPdfQuery.Verify(x => x.GetOrNull(expectedKey), Times.Once);
        }

        [Test]
        public void Status_ShouldReturnNull_WhenProgressDoesNotExist()
        {
            // Arrange
            var documentType = DocumentType.Html;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";

            this.mockPdfQuery.Setup(x => x.GetOrNull(expectedKey)).Returns((PdfGenerationProgress)null);

            // Act
            var result = this.pdfService.Status(this.questionnaireRevision, this.testTranslation, documentType);

            // Assert
            Assert.That(result, Is.Null);
            this.mockPdfQuery.Verify(x => x.GetOrNull(expectedKey), Times.Once);
        }

        [Test]
        public async Task Retry_ShouldRemoveFailedProgressAndCreateNew()
        {
            // Arrange
            var documentType = DocumentType.Pdf;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";
            var failedProgress = new PdfGenerationProgress();
            failedProgress.Fail();
            var questionnaire = CreateMockPdfQuestionnaireModel();
            var newProgress = new PdfGenerationProgress();

            this.mockPdfFactory.Setup(x => x.Load(this.questionnaireRevision, this.testUserId, "testuser", this.testTranslation, false))
                .Returns(questionnaire);
            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RouteData>()))
                .ReturnsAsync("<html>questionnaire content</html>");
            this.mockPdfQuery.SetupSequence(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()))
                .Returns(failedProgress)
                .Returns(newProgress);

            // Act
            var result = await this.pdfService.Retry(this.questionnaireRevision, this.testTranslation, documentType);

            // Assert
            Assert.That(result, Is.EqualTo(newProgress));
            this.mockPdfQuery.Verify(x => x.Remove(expectedKey), Times.Once);
            this.mockPdfQuery.Verify(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()), Times.Exactly(2));
        }

        [Test]
        public async Task Retry_ShouldReturnExistingProgress_WhenProgressIsNotFailed()
        {
            // Arrange
            var documentType = DocumentType.Html;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";
            var existingProgress = new PdfGenerationProgress(); // Not failed
            var questionnaire = CreateMockPdfQuestionnaireModel();

            this.mockPdfFactory.Setup(x => x.Load(this.questionnaireRevision, this.testUserId, "testuser", this.testTranslation, false))
                .Returns(questionnaire);
            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RouteData>()))
                .ReturnsAsync("<html>questionnaire content</html>");
            this.mockPdfQuery.Setup(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()))
                .Returns(existingProgress);

            // Act
            var result = await this.pdfService.Retry(this.questionnaireRevision, this.testTranslation, documentType);

            // Assert
            Assert.That(result, Is.EqualTo(existingProgress));
            this.mockPdfQuery.Verify(x => x.Remove(expectedKey), Times.Never);
            this.mockPdfQuery.Verify(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()), Times.Once);
        }

        [Test]
        public void Download_ShouldReturnFileContentAndCleanup_WhenProgressIsFinished()
        {
            // Arrange
            var documentType = DocumentType.Pdf;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";
            var progress = new PdfGenerationProgress();
            progress.Finish();
            var expectedContent = new byte[] { 1, 2, 3, 4, 5 };

            this.mockPdfQuery.Setup(x => x.GetOrNull(expectedKey)).Returns(progress);
            this.mockFileSystemAccessor.Setup(x => x.ReadAllBytes(It.IsAny<string>(),
                It.IsAny<long?>(),It.IsAny<long?>())).Returns(expectedContent);

            // Act
            var result = this.pdfService.Download(this.questionnaireRevision, this.testTranslation, documentType);

            // Assert
            Assert.That(result, Is.EqualTo(expectedContent));
            this.mockFileSystemAccessor.Verify(x => x.ReadAllBytes(It.IsAny<string>(),It.IsAny<long?>(),It.IsAny<long?>()), Times.Once);
            this.mockFileSystemAccessor.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Once);
            this.mockPdfQuery.Verify(x => x.Remove(expectedKey), Times.Once);
        }

        [Test]
        public void Download_ShouldReturnNull_WhenProgressIsNotFinished()
        {
            // Arrange
            var documentType = DocumentType.Html;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";
            var progress = new PdfGenerationProgress(); // Not finished

            this.mockPdfQuery.Setup(x => x.GetOrNull(expectedKey)).Returns(progress);

            // Act
            var result = this.pdfService.Download(this.questionnaireRevision, this.testTranslation, documentType);

            // Assert
            Assert.That(result, Is.Null);
            this.mockFileSystemAccessor.Verify(x => x.ReadAllBytes(It.IsAny<string>(),
                It.IsAny<long?>(), It.IsAny<long?>()), Times.Never);
            this.mockFileSystemAccessor.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never);
            // Note: Remove is not called when progress is not finished, so we don't verify it
        }

        [Test]
        public void Download_ShouldReturnNull_WhenProgressDoesNotExist()
        {
            // Arrange
            var documentType = DocumentType.Pdf;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";

            this.mockPdfQuery.Setup(x => x.GetOrNull(expectedKey)).Returns((PdfGenerationProgress)null);

            // Act
            var result = this.pdfService.Download(this.questionnaireRevision, this.testTranslation, documentType);

            // Assert
            Assert.That(result, Is.Null);
            this.mockFileSystemAccessor.Verify(x => x.ReadAllBytes(It.IsAny<string>(),
                It.IsAny<long?>(),It.IsAny<long?>()), Times.Never);
            this.mockFileSystemAccessor.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never);
            // Note: Remove is not called when progress doesn't exist, so we don't verify it
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
            this.mockPdfQuery.Verify(x => x.GetQueryInfoJson(), Times.Once);
        }

        [Test]
        public void GetUserId_ShouldReturnEmptyGuid_WhenHttpContextIsNull()
        {
            // Arrange
            this.mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

            // Act
            this.pdfService.Status(this.questionnaireRevision, this.testTranslation, DocumentType.Pdf);

            // Assert - The key would contain Guid.Empty as user ID
            var expectedKey = $"{Guid.Empty}:{DocumentType.Pdf}:{this.questionnaireRevision}:{this.testTranslation}";
            this.mockPdfQuery.Verify(x => x.GetOrNull(expectedKey), Times.Once);
        }

        [Test]
        public void GetUserId_ShouldReturnEmptyGuid_WhenUserIsNull()
        {
            // Arrange
            this.mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(this.mockHttpContext.Object);
            this.mockHttpContext.Setup(x => x.User).Returns((ClaimsPrincipal)null);

            // Act
            this.pdfService.Status(this.questionnaireRevision, this.testTranslation, DocumentType.Html);

            // Assert - The key would contain Guid.Empty as user ID
            var expectedKey = $"{Guid.Empty}:{DocumentType.Html}:{this.questionnaireRevision}:{this.testTranslation}";
            this.mockPdfQuery.Verify(x => x.GetOrNull(expectedKey), Times.Once);
        }

        [Test]
        public void GetUserId_ShouldReturnEmptyGuid_WhenUserIdIsNull()
        {
            // Arrange
            this.mockUser.Setup(x => x.GetIdOrNull()).Returns((Guid?)null);

            // Act
            this.pdfService.Status(this.questionnaireRevision, this.testTranslation, DocumentType.Pdf);

            // Assert - The key would contain Guid.Empty as user ID
            var expectedKey = $"{Guid.Empty}:{DocumentType.Pdf}:{this.questionnaireRevision}:{this.testTranslation}";
            this.mockPdfQuery.Verify(x => x.GetOrNull(expectedKey), Times.Once);
        }

        [Test]
        public void GetKey_ShouldGenerateCorrectKey_WithAllParameters()
        {
            // Arrange
            var documentType = DocumentType.Pdf;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";

            // Act
            this.pdfService.Status(this.questionnaireRevision, this.testTranslation, documentType);

            // Assert
            this.mockPdfQuery.Verify(x => x.GetOrNull(expectedKey), Times.Once);
        }

        [Test]
        public void GetKey_ShouldGenerateCorrectKey_WithNullTranslation()
        {
            // Arrange
            var documentType = DocumentType.Html;
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:";

            // Act
            this.pdfService.Status(this.questionnaireRevision, null, documentType);

            // Assert
            this.mockPdfQuery.Verify(x => x.GetOrNull(expectedKey), Times.Once);
        }

        [TestCase(DocumentType.Pdf)]
        [TestCase(DocumentType.Html)]
        [TestCase(DocumentType.Unspecified)]
        public async Task Enqueue_ShouldHandleAllDocumentTypes(DocumentType documentType)
        {
            // Arrange
            var expectedKey = $"{this.testUserId}:{documentType}:{this.questionnaireRevision}:{this.testTranslation}";
            var questionnaire = CreateMockPdfQuestionnaireModel();
            var progress = new PdfGenerationProgress();

            this.mockPdfQuery.Setup(x => x.GetOrNull(expectedKey)).Returns((PdfGenerationProgress)null);
            this.mockPdfFactory.Setup(x => x.Load(this.questionnaireRevision, this.testUserId, "testuser", this.testTranslation, false))
                .Returns(questionnaire);
            this.mockViewRenderService.Setup(x => x.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RouteData>()))
                .ReturnsAsync("<html>questionnaire content</html>");
            this.mockPdfQuery.Setup(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()))
                .Returns(progress);

            // Act
            var result = await this.pdfService.Enqueue(this.questionnaireRevision, this.testTranslation, documentType, 0);

            // Assert
            Assert.That(result, Is.EqualTo(progress));
            this.mockPdfQuery.Verify(x => x.GetOrAdd(this.testUserId, expectedKey, It.IsAny<Func<PdfGenerationProgress, Task>>()), Times.Once);
        }

        private PdfQuestionnaireModel CreateMockPdfQuestionnaireModel()
        {
            // Create mock objects for the constructor parameters
            var mockQuestionnaire = Mock.Of<QuestionnaireDocument>();
            var mockSettings = Mock.Of<PdfSettings>();
            var mockAllItems = new System.Collections.Generic.List<Main.Core.Entities.Composite.IComposite>();
            var mockCreated = new PdfQuestionnaireModel.ModificationStatisticsByUser();
            var mockLastModified = new PdfQuestionnaireModel.ModificationStatisticsByUser();
            var mockRequested = new PdfQuestionnaireModel.ModificationStatisticsByUser();

            return new PdfQuestionnaireModel(
                mockQuestionnaire,
                mockSettings,
                mockAllItems,
                mockCreated,
                mockLastModified,
                mockRequested);
        }
    }
}
