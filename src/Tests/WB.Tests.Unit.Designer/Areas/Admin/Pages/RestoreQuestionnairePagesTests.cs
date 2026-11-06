using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.UI.Designer.Areas.Admin.Pages;
using WB.UI.Designer.Code.ImportExport;
using WB.UI.Designer.Services.Restore;

namespace WB.Tests.Unit.Designer.Areas.Admin.Pages
{
    [TestFixture]
    public class RestoreQuestionnairePagesTests
    {
        // A restore deletes the existing translations and categories before rewriting them. Both pages swallow
        // failures and return a 200 page, so without an explicit rollback mark TransactionFilter would commit
        // the deletions of a restore that never completed.

        [Test]
        public void RestoreQuestionnaire_when_restore_throws_marks_request_rollback_only()
        {
            var restoreService = new Mock<IQuestionnaireImportService>();
            restoreService
                .Setup(s => s.RestoreQuestionnaire(It.IsAny<Stream>(), It.IsAny<Guid>(), It.IsAny<RestoreState>(), It.IsAny<bool>()))
                .Throws(new InvalidOperationException("boom"));

            var rollbackState = new TransactionRollbackState();
            var page = new RestoreQuestionnaireModel(
                Mock.Of<ILogger<RestoreQuestionnaireModel>>(), restoreService.Object, rollbackState)
            {
                Upload = CreateZipUpload()
            };
            SetupPageContext(page);

            page.OnPost();

            Assert.That(rollbackState.IsRollbackOnly, Is.True);
            Assert.That(page.Success, Is.Null);
        }

        [Test]
        public void RestoreQuestionnaire_when_restore_succeeds_does_not_mark_request_rollback_only()
        {
            var restoreService = new Mock<IQuestionnaireImportService>();
            restoreService
                .Setup(s => s.RestoreQuestionnaire(It.IsAny<Stream>(), It.IsAny<Guid>(), It.IsAny<RestoreState>(), It.IsAny<bool>()))
                .Returns(Guid.NewGuid());

            var rollbackState = new TransactionRollbackState();
            var page = new RestoreQuestionnaireModel(
                Mock.Of<ILogger<RestoreQuestionnaireModel>>(), restoreService.Object, rollbackState)
            {
                Upload = CreateZipUpload()
            };
            SetupPageContext(page);

            page.OnPost();

            Assert.That(rollbackState.IsRollbackOnly, Is.False);
            Assert.That(page.Success, Is.Not.Null);
        }

        [Test]
        public void RestoreQuestionnaireDocument_when_restore_reports_swallowed_failure_marks_request_rollback_only()
        {
            var restoreService = new Mock<IQuestionnaireRestoreService>();
            restoreService
                .Setup(s => s.RestoreQuestionnaire(It.IsAny<Stream>(), It.IsAny<Guid>(), It.IsAny<RestoreState>(), It.IsAny<bool>()))
                .Returns((Stream _, Guid __, RestoreState state, bool ___) =>
                {
                    state.MarkFailed();
                    state.Error = "Error processing zip file entry";
                    return Guid.NewGuid();
                });

            var rollbackState = new TransactionRollbackState();
            var page = new RestoreQuestionnaireDocumentModel(
                Mock.Of<ILogger<RestoreQuestionnaireDocumentModel>>(), restoreService.Object, rollbackState)
            {
                Upload = CreateZipUpload()
            };
            SetupPageContext(page);

            page.OnPost();

            Assert.That(rollbackState.IsRollbackOnly, Is.True);
            Assert.That(page.Success, Is.Null);
        }

        [Test]
        public void RestoreQuestionnaireDocument_when_restore_reports_unrestored_attachment_marks_request_rollback_only()
        {
            // A pending attachment is reported through Error only, yet the stored document still references it.
            var restoreService = new Mock<IQuestionnaireRestoreService>();
            restoreService
                .Setup(s => s.RestoreQuestionnaire(It.IsAny<Stream>(), It.IsAny<Guid>(), It.IsAny<RestoreState>(), It.IsAny<bool>()))
                .Returns((Stream _, Guid __, RestoreState state, bool ___) =>
                {
                    state.Error = "Attachment 'x' was not restored because there are not enough data for it in it's folder.";
                    return Guid.NewGuid();
                });

            var rollbackState = new TransactionRollbackState();
            var page = new RestoreQuestionnaireDocumentModel(
                Mock.Of<ILogger<RestoreQuestionnaireDocumentModel>>(), restoreService.Object, rollbackState)
            {
                Upload = CreateZipUpload()
            };
            SetupPageContext(page);

            page.OnPost();

            Assert.That(rollbackState.IsRollbackOnly, Is.True);
            Assert.That(page.Success, Is.Null);
        }

        [Test]
        public void RestoreQuestionnaireDocument_when_restore_throws_marks_request_rollback_only()
        {
            var restoreService = new Mock<IQuestionnaireRestoreService>();
            restoreService
                .Setup(s => s.RestoreQuestionnaire(It.IsAny<Stream>(), It.IsAny<Guid>(), It.IsAny<RestoreState>(), It.IsAny<bool>()))
                .Throws(new InvalidOperationException("boom"));

            var rollbackState = new TransactionRollbackState();
            var page = new RestoreQuestionnaireDocumentModel(
                Mock.Of<ILogger<RestoreQuestionnaireDocumentModel>>(), restoreService.Object, rollbackState)
            {
                Upload = CreateZipUpload()
            };
            SetupPageContext(page);

            page.OnPost();

            Assert.That(rollbackState.IsRollbackOnly, Is.True);
            Assert.That(page.Success, Is.Null);
        }

        private static void SetupPageContext(PageModel page)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));

            page.PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private static IFormFile CreateZipUpload()
        {
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("backup.zip");
            file.Setup(f => f.Length).Returns(1);
            file.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(new byte[] { 1 }));
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return file.Object;
        }
    }
}
