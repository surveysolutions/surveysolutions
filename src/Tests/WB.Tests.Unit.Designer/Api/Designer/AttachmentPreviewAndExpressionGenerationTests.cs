#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Services.AttachmentPreview;

namespace WB.Tests.Unit.Designer.Api.Designer
{
    [TestFixture]
    [TestOf(typeof(AttachmentPreviewHelper))]
    public class AttachmentPreviewHelperTests
    {
        private string webRoot = null!;
        private AttachmentPreviewHelper helper = null!;

        [SetUp]
        public void SetUp()
        {
            webRoot = Path.Combine(Path.GetTempPath(), "preview-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(webRoot, "images"));
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(x => x.WebRootPath).Returns(webRoot);
            helper = new AttachmentPreviewHelper(env.Object);
        }

        [TearDown]
        public void TearDown() => Directory.Delete(webRoot, true);

        private static byte[] Png(int width, int height)
        {
            using var image = new Image<Rgba32>(width, height);
            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            return stream.ToArray();
        }

        [Test]
        public void when_content_is_null_should_return_null()
        {
            helper.GetPreviewImage(new AttachmentContent { Content = null, ContentType = "image/png" }, null).Should().BeNull();
        }

        [Test]
        public void when_no_size_requested_should_return_original_content_and_type()
        {
            var content = new byte[] { 1, 2, 3 };

            var result = helper.GetPreviewImage(new AttachmentContent { Content = content, ContentType = "video/mp4" }, null)!;

            result.Content.Should().Equal(content);
            result.ContentType.Should().Be("video/mp4");
        }

        [Test]
        public void when_image_and_size_requested_should_resize_preserving_aspect_ratio()
        {
            var content = new AttachmentContent { Content = Png(200, 100), ContentType = "image/png" };

            var result = helper.GetPreviewImage(content, 50)!;

            result.ContentType.Should().Be("image/jpg");
            using var resized = Image.Load(result.Content);
            resized.Width.Should().Be(50);
            resized.Height.Should().Be(25);
        }

        [Test]
        public void when_stored_thumbnail_exists_should_use_it()
        {
            var content = new AttachmentContent { Content = new byte[] { 1 }, ContentType = "video/mp4" };
            content.Details.Thumbnail = Png(80, 80);

            var result = helper.GetPreviewImage(content, 40)!;

            using var resized = Image.Load(result.Content);
            resized.Width.Should().Be(40);
            resized.Height.Should().Be(40);
        }

        [Test]
        public void when_audio_should_return_icon_without_resizing()
        {
            var icon = new byte[] { 7, 7, 7 };
            File.WriteAllBytes(Path.Combine(webRoot, "images", "icons-files-audio.png"), icon);

            var result = helper.GetPreviewImage(new AttachmentContent { Content = new byte[] { 1 }, ContentType = "audio/mp3" }, 40)!;

            result.ContentType.Should().Be("image/png");
            result.Content.Should().Equal(icon);
        }

        [Test]
        public void when_pdf_should_return_icon_without_resizing()
        {
            var icon = new byte[] { 8, 8 };
            File.WriteAllBytes(Path.Combine(webRoot, "images", "icons-files-pdf.png"), icon);

            var result = helper.GetPreviewImage(new AttachmentContent { Content = new byte[] { 1 }, ContentType = "application/pdf" }, 40)!;

            result.ContentType.Should().Be("image/png");
            result.Content.Should().Equal(icon);
        }

        [Test]
        public void when_video_without_thumbnail_and_size_requested_should_return_null()
        {
            helper.GetPreviewImage(new AttachmentContent { Content = new byte[] { 1 }, ContentType = "video/mp4" }, 40)
                .Should().BeNull();
        }
    }

    [TestFixture]
    [TestOf(typeof(ExpressionGenerationController))]
    public class ExpressionGenerationControllerTests
    {
        private Mock<IQuestionnaireVerifier> verifier = null!;
        private Mock<IExpressionProcessorGenerator> generator = null!;
        private Mock<IQuestionnaireViewFactory> views = null!;
        private Mock<IDesignerEngineVersionService> versions = null!;
        private Mock<IQuestionnaireCompilationVersionService> compilation = null!;
        private Mock<IQuestionnaireCodeGenerationPackageFactory> packages = null!;
        private ExpressionGenerationController controller = null!;
        private readonly Guid id = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            verifier = new Mock<IQuestionnaireVerifier>();
            generator = new Mock<IExpressionProcessorGenerator>();
            views = new Mock<IQuestionnaireViewFactory>();
            versions = new Mock<IDesignerEngineVersionService>();
            versions.SetupGet(x => x.LatestSupportedVersion).Returns(30);
            compilation = new Mock<IQuestionnaireCompilationVersionService>();
            packages = new Mock<IQuestionnaireCodeGenerationPackageFactory>();
            controller = new ExpressionGenerationController(generator.Object, views.Object, versions.Object,
                compilation.Object, verifier.Object, packages.Object);
        }

        private void QuestionnaireExists()
            => views.Setup(x => x.Load(It.IsAny<QuestionnaireViewInputModel>())).Returns(Create.QuestionnaireView());

        private void Compiles(IEnumerable<QuestionnaireVerificationMessage> messages, string assembly = "")
        {
            var asm = assembly;
            verifier.Setup(x => x.CompileAndVerify(It.IsAny<QuestionnaireView>(), It.IsAny<int?>(), out asm)).Returns(messages);
        }

        [Test]
        public void when_questionnaire_does_not_exist_should_throw()
        {
            views.Setup(x => x.Load(It.IsAny<QuestionnaireViewInputModel>())).Returns((QuestionnaireView?)null);

            Assert.Throws<Exception>(() => controller.GetCompilationResultForLatestVersion(id));
        }

        [Test]
        public void when_compilation_has_only_warnings_should_report_no_errors()
        {
            QuestionnaireExists();
            Compiles(new[] { QuestionnaireVerificationMessage.Warning("W1", "warn") });

            controller.GetCompilationResultForLatestVersion(id).Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be("No errors");
        }

        [Test]
        public void when_compilation_has_errors_should_return_precondition_failed_with_messages()
        {
            QuestionnaireExists();
            Compiles(new[] { QuestionnaireVerificationMessage.Error("E1", "broken") });

            var result = controller.GetCompilationResultForLatestVersion(id).Should().BeOfType<ObjectResult>().Subject;

            result.StatusCode.Should().Be(412);
            ((string[])result.Value!).Should().Equal("broken");
        }

        [Test]
        public void when_compilation_version_is_specified_should_use_it_instead_of_latest()
        {
            QuestionnaireExists();
            compilation.Setup(x => x.GetById(id)).Returns(new QuestionnaireCompilationVersion { QuestionnaireId = id, Version = 12 });
            Compiles(Array.Empty<QuestionnaireVerificationMessage>());

            controller.GetCompilationResultForLatestVersion(id);

            string asm;
            verifier.Verify(x => x.CompileAndVerify(It.IsAny<QuestionnaireView>(), 12, out asm), Times.Once);
        }

        [Test]
        public void when_no_compilation_version_specified_should_use_latest_supported()
        {
            QuestionnaireExists();
            Compiles(Array.Empty<QuestionnaireVerificationMessage>());

            controller.GetCompilationResultForLatestVersion(id);

            string asm;
            verifier.Verify(x => x.CompileAndVerify(It.IsAny<QuestionnaireView>(), 30, out asm), Times.Once);
        }

        [Test]
        public void when_assembly_compiles_should_return_dll_file()
        {
            QuestionnaireExists();
            Compiles(Array.Empty<QuestionnaireVerificationMessage>(), Convert.ToBase64String(new byte[] { 1, 2, 3 }));

            var file = controller.GetLatestVersionAssembly(id, null).Should().BeOfType<FileContentResult>().Subject;

            file.FileContents.Should().Equal(1, 2, 3);
            file.FileDownloadName.Should().Be($"expressions-{id}.dll");
            file.ContentType.Should().Be("application/x-msdownload");
        }

        [Test]
        public void when_assembly_has_errors_should_return_precondition_failed()
        {
            QuestionnaireExists();
            Compiles(new[] { QuestionnaireVerificationMessage.Critical("C1", "fatal") });

            var result = controller.GetLatestVersionAssembly(id, 25).Should().BeOfType<ObjectResult>().Subject;

            result.StatusCode.Should().Be(412);
            ((string)result.Value!).Should().Contain("fatal");
        }

        [Test]
        public void when_generating_classes_should_concatenate_files_with_headers()
        {
            QuestionnaireExists();
            generator.Setup(x => x.GenerateProcessorStateClasses(It.IsAny<QuestionnaireCodeGenerationPackage>(), 18, true))
                .Returns(new Dictionary<string, string> { ["A.cs"] = "class A {}", ["B.cs"] = "class B {}" });

            var text = controller.GetAllClassesForLatestVersion(id, 18).Should().BeOfType<OkObjectResult>().Subject.Value!.ToString()!;

            text.Should().Contain("//A.cs").And.Contain("class A {}").And.Contain("//B.cs").And.Contain("class B {}");
        }

        [Test]
        public void when_generating_classes_without_version_should_use_latest_supported()
        {
            QuestionnaireExists();
            generator.Setup(x => x.GenerateProcessorStateClasses(It.IsAny<QuestionnaireCodeGenerationPackage>(), It.IsAny<int>(), true))
                .Returns(new Dictionary<string, string>());

            controller.GetAllClassesForLatestVersion(id, null);

            generator.Verify(x => x.GenerateProcessorStateClasses(It.IsAny<QuestionnaireCodeGenerationPackage>(), 30, true), Times.Once);
        }
    }
}

