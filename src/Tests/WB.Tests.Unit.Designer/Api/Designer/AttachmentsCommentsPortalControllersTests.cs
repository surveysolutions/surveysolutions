#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Portal;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Models;
using WB.UI.Designer.Services.AttachmentPreview;
using Attachment = WB.Core.SharedKernels.SurveySolutions.Documents.Attachment;
using QuestionnaireDocument = Main.Core.Documents.QuestionnaireDocument;

namespace WB.Tests.Unit.Designer.Api.Designer
{
    internal static class ControllerTestUser
    {
        public static ClaimsPrincipal Principal(Guid? id = null, bool admin = false, bool authenticated = true)
        {
            var identity = authenticated ? new ClaimsIdentity("test") : new ClaimsIdentity();
            if (id.HasValue)
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id.Value.ToString()));
            if (admin)
                identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
            return new ClaimsPrincipal(identity);
        }

        public static Mock<UserManager<DesignerIdentityUser>> UserManager()
            => new Mock<UserManager<DesignerIdentityUser>>(
                new Mock<IUserStore<DesignerIdentityUser>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [TestFixture]
    [TestOf(typeof(AttachmentsController))]
    public class AttachmentsControllerTests
    {
        private Mock<IAttachmentService> attachments = null!;
        private Mock<IDesignerQuestionnaireStorage> storage = null!;
        private Mock<IAttachmentPreviewHelper> preview = null!;
        private AttachmentsController controller = null!;
        private readonly Guid attachmentId = Guid.NewGuid();
        private readonly QuestionnaireRevision revision = new QuestionnaireRevision(Guid.NewGuid());

        [SetUp]
        public void SetUp()
        {
            attachments = new Mock<IAttachmentService>();
            storage = new Mock<IDesignerQuestionnaireStorage>();
            preview = new Mock<IAttachmentPreviewHelper>();
            controller = new AttachmentsController(attachments.Object, Mock.Of<IWebHostEnvironment>(), storage.Object, preview.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        private void QuestionnaireHasAttachment()
        {
            var doc = new QuestionnaireDocument();
            doc.Attachments.Add(new Attachment { AttachmentId = attachmentId, ContentId = "c1" });
            storage.Setup(x => x.Get(revision)).Returns(doc);
        }

        private void MetaAndContent(byte[]? content = null)
        {
            attachments.Setup(x => x.GetAttachmentMeta(attachmentId))
                .Returns(new AttachmentMeta { AttachmentId = attachmentId, ContentId = "c1", FileName = "pic.png", LastUpdateDate = DateTime.UtcNow });
            attachments.Setup(x => x.GetContent("c1"))
                .Returns(new AttachmentContent { ContentId = "c1", Content = content, ContentType = "image/png" });
        }

        [Test]
        public void when_questionnaire_not_found_should_return_not_found()
        {
            storage.Setup(x => x.Get(revision)).Returns((QuestionnaireDocument?)null);

            controller.Get(revision, attachmentId).Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_attachment_does_not_belong_to_questionnaire_should_return_not_found()
        {
            storage.Setup(x => x.Get(revision)).Returns(new QuestionnaireDocument());
            MetaAndContent(new byte[] { 1 });

            controller.Get(revision, attachmentId).Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_attachment_meta_missing_should_return_not_found()
        {
            QuestionnaireHasAttachment();
            attachments.Setup(x => x.GetAttachmentMeta(attachmentId)).Returns((AttachmentMeta?)null);

            controller.Get(revision, attachmentId).Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_etag_matches_should_return_not_modified()
        {
            QuestionnaireHasAttachment();
            MetaAndContent(new byte[] { 1 });
            controller.Request.Headers[HeaderNames.IfNoneMatch] = "\"c1\"";

            controller.Get(revision, attachmentId).Should().BeOfType<StatusCodeResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status304NotModified);
        }

        [Test]
        public void when_content_record_missing_should_return_not_found()
        {
            QuestionnaireHasAttachment();
            attachments.Setup(x => x.GetAttachmentMeta(attachmentId))
                .Returns(new AttachmentMeta { ContentId = "c1", FileName = "f" });
            attachments.Setup(x => x.GetContent("c1")).Returns((AttachmentContent?)null);

            controller.Get(revision, attachmentId).Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_content_bytes_missing_should_return_no_content()
        {
            QuestionnaireHasAttachment();
            MetaAndContent(null);

            controller.Get(revision, attachmentId).Should().BeOfType<NoContentResult>();
        }

        [Test]
        public void when_preview_not_available_should_return_no_content()
        {
            QuestionnaireHasAttachment();
            MetaAndContent(new byte[] { 1 });
            preview.Setup(x => x.GetPreviewImage(It.IsAny<AttachmentContent>(), It.IsAny<int?>()))
                .Returns((AttachmentPreviewContent?)null);

            controller.Get(revision, attachmentId).Should().BeOfType<NoContentResult>();
        }

        [Test]
        public void when_attachment_available_should_return_file_with_etag()
        {
            QuestionnaireHasAttachment();
            MetaAndContent(new byte[] { 1, 2 });
            preview.Setup(x => x.GetPreviewImage(It.IsAny<AttachmentContent>(), null))
                .Returns(new AttachmentPreviewContent("image/png", new byte[] { 9 }));

            var file = controller.Get(revision, attachmentId).Should().BeOfType<FileContentResult>().Subject;

            file.ContentType.Should().Be("image/png");
            file.FileDownloadName.Should().Be("pic.png");
            file.EntityTag!.Tag.ToString().Should().Be("\"c1\"");
            file.FileContents.Should().Equal(9);
        }

        [Test]
        public void when_thumbnail_requested_without_size_should_scale_to_default()
        {
            QuestionnaireHasAttachment();
            MetaAndContent(new byte[] { 1 });
            preview.Setup(x => x.GetPreviewImage(It.IsAny<AttachmentContent>(), It.IsAny<int?>()))
                .Returns(new AttachmentPreviewContent("image/png", new byte[] { 9 }));

            controller.Thumbnail(revision, attachmentId);

            preview.Verify(x => x.GetPreviewImage(It.IsAny<AttachmentContent>(), 156), Times.Once);
        }

        [Test]
        public void when_thumbnail_requested_with_size_should_scale_to_it()
        {
            QuestionnaireHasAttachment();
            MetaAndContent(new byte[] { 1 });
            preview.Setup(x => x.GetPreviewImage(It.IsAny<AttachmentContent>(), It.IsAny<int?>()))
                .Returns(new AttachmentPreviewContent("image/png", new byte[] { 9 }));

            controller.Thumbnail(revision, attachmentId, 64);

            preview.Verify(x => x.GetPreviewImage(It.IsAny<AttachmentContent>(), 64), Times.Once);
        }

#pragma warning disable CS0618
        [Test]
        public void when_legacy_thumbnail_requested_should_skip_questionnaire_check()
        {
            MetaAndContent(new byte[] { 1 });
            preview.Setup(x => x.GetPreviewImage(It.IsAny<AttachmentContent>(), It.IsAny<int?>()))
                .Returns(new AttachmentPreviewContent("image/png", new byte[] { 9 }));

            controller.Thumbnail(attachmentId).Should().BeOfType<FileContentResult>();

            storage.Verify(x => x.Get(It.IsAny<QuestionnaireRevision>()), Times.Never);
        }
#pragma warning restore CS0618
    }

    [TestFixture]
    [TestOf(typeof(CommentsController))]
    public class CommentsControllerTests
    {
        private Mock<ICommentsService> comments = null!;
        private Mock<IQuestionnaireViewFactory> views = null!;
        private Mock<UserManager<DesignerIdentityUser>> users = null!;
        private DesignerDbContext db = null!;
        private CommentsController controller = null!;
        private readonly Guid userId = Guid.NewGuid();
        private readonly Guid questionnaireId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            comments = new Mock<ICommentsService>();
            views = new Mock<IQuestionnaireViewFactory>();
            users = ControllerTestUser.UserManager();
            db = Create.InMemoryDbContext();
            controller = new CommentsController(comments.Object, views.Object, db, users.Object);
        }

        private void Login(bool admin = false, bool authenticated = true)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = ControllerTestUser.Principal(userId, admin, authenticated) }
            };
        }

        [Test]
        public void when_anonymous_requests_threads_should_return_empty()
        {
            Login(authenticated: false);

            controller.commentThreads(new QuestionnaireRevision(questionnaireId)).Should().BeEmpty();
            comments.Verify(x => x.LoadCommentThreads(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void when_user_has_no_access_should_return_empty_threads()
        {
            Login();
            views.Setup(x => x.HasUserChangeAccessToQuestionnaire(questionnaireId, userId)).Returns(false);

            controller.commentThreads(new QuestionnaireRevision(questionnaireId)).Should().BeEmpty();
        }

        [Test]
        public void when_user_has_access_should_return_threads()
        {
            Login();
            views.Setup(x => x.HasUserChangeAccessToQuestionnaire(questionnaireId, userId)).Returns(true);
            var threads = new List<CommentThread> { new CommentThread(new CommentView[0], null) };
            comments.Setup(x => x.LoadCommentThreads(questionnaireId)).Returns(threads);

            controller.commentThreads(new QuestionnaireRevision(questionnaireId)).Should().BeSameAs(threads);
        }

        [Test]
        public void when_admin_should_return_threads_without_access_check()
        {
            Login(admin: true);
            var threads = new List<CommentThread> { new CommentThread(new CommentView[0], null) };
            comments.Setup(x => x.LoadCommentThreads(questionnaireId)).Returns(threads);

            controller.commentThreads(new QuestionnaireRevision(questionnaireId)).Should().BeSameAs(threads);
            views.Verify(x => x.HasUserChangeAccessToQuestionnaire(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task when_anonymous_requests_entity_comments_should_return_empty()
        {
            Login(authenticated: false);

            (await controller.Get(new QuestionnaireRevision(questionnaireId), Guid.NewGuid())).Should().BeEmpty();
        }

        [Test]
        public async Task when_user_has_access_should_load_entity_comments()
        {
            Login();
            var entityId = Guid.NewGuid();
            views.Setup(x => x.HasUserChangeAccessToQuestionnaire(questionnaireId, userId)).Returns(true);
            var list = new List<CommentView> { new CommentView { Comment = "hi" } };
            comments.Setup(x => x.LoadCommentsForEntity(questionnaireId, entityId)).ReturnsAsync(list);

            (await controller.Get(new QuestionnaireRevision(questionnaireId), entityId)).Should().BeSameAs(list);
        }

        [Test]
        public async Task when_user_has_no_access_should_not_load_entity_comments()
        {
            Login();
            views.Setup(x => x.HasUserChangeAccessToQuestionnaire(questionnaireId, userId)).Returns(false);

            (await controller.Get(new QuestionnaireRevision(questionnaireId), Guid.NewGuid())).Should().BeEmpty();
            comments.Verify(x => x.LoadCommentsForEntity(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task when_model_state_invalid_should_return_error_and_not_post()
        {
            Login();
            controller.ModelState.AddModelError("Comment", "required");

            var result = await controller.PostComment(new QuestionnaireRevision(questionnaireId), new AddCommentModel());

            result.Should().BeOfType<JsonResult>().Which.Value!.ToString().Should().Contain("required");
            comments.Verify(x => x.PostComment(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task when_posting_to_a_historical_revision_should_be_denied()
        {
            Login(admin: true);

            var result = await controller.PostComment(new QuestionnaireRevision(questionnaireId, Guid.NewGuid()), new AddCommentModel());

            result.Should().BeOfType<JsonResult>().Which.Value!.ToString().Should().Contain("Access denied");
            comments.Verify(x => x.PostComment(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task when_posting_without_questionnaire_access_should_be_denied()
        {
            Login();
            views.Setup(x => x.HasUserChangeAccessToQuestionnaire(questionnaireId, userId)).Returns(false);

            var result = await controller.PostComment(new QuestionnaireRevision(questionnaireId), new AddCommentModel());

            result.Should().BeOfType<JsonResult>().Which.Value!.ToString().Should().Contain("Access denied");
        }

        [Test]
        public async Task when_user_cannot_be_resolved_should_be_denied()
        {
            Login(admin: true);
            users.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((DesignerIdentityUser?)null);

            var result = await controller.PostComment(new QuestionnaireRevision(questionnaireId), new AddCommentModel());

            result.Should().BeOfType<JsonResult>().Which.Value!.ToString().Should().Contain("Access denied");
        }

        [Test]
        public async Task when_posting_allowed_should_store_comment_with_user_details()
        {
            Login(admin: true);
            users.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new DesignerIdentityUser { UserName = "john", Email = "j@x.com" });
            var model = new AddCommentModel { Id = Guid.NewGuid(), EntityId = Guid.NewGuid(), QuestionnaireId = questionnaireId, Comment = "text" };

            var result = await controller.PostComment(new QuestionnaireRevision(questionnaireId), model);

            result.Should().BeOfType<OkResult>();
            comments.Verify(x => x.PostComment(model.Id, questionnaireId, model.EntityId, "text", "john", "j@x.com"), Times.Once);
        }

        [Test]
        public async Task when_resolving_missing_comment_should_return_not_found()
        {
            comments.Setup(x => x.ResolveCommentAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ThrowsAsync(new InvalidOperationException());

            (await controller.ResolveComment(questionnaireId, Guid.NewGuid())).Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task when_resolving_comment_should_return_ok()
        {
            var commentId = Guid.NewGuid();

            (await controller.ResolveComment(questionnaireId, commentId)).Should().BeOfType<OkResult>();
            comments.Verify(x => x.ResolveCommentAsync(commentId, questionnaireId), Times.Once);
        }

        [Test]
        public async Task when_deleting_comment_should_delegate_and_return_ok()
        {
            var commentId = Guid.NewGuid();

            (await controller.DeleteComment(questionnaireId, commentId)).Should().BeOfType<OkResult>();
            comments.Verify(x => x.DeleteCommentAsync(commentId, questionnaireId), Times.Once);
        }
    }

    [TestFixture]
    [TestOf(typeof(PortalController))]
    public class PortalControllerTests
    {
        private Mock<UserManager<DesignerIdentityUser>> users = null!;
        private Mock<IQuestionnaireHelper> helper = null!;
        private PortalController controller = null!;

        [SetUp]
        public void SetUp()
        {
            users = ControllerTestUser.UserManager();
            helper = new Mock<IQuestionnaireHelper>();
            controller = new PortalController(users.Object, helper.Object);
        }

        [TestCase("")]
        [TestCase("  ")]
        public async Task when_user_id_is_blank_should_return_bad_request(string userId)
        {
            (await controller.GetUserInfo(userId)).Should().BeOfType<BadRequestObjectResult>();
            (await controller.QuestionnairesForUser(userId, "")).Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task when_user_not_found_should_return_not_found()
        {
            (await controller.GetUserInfo("nobody")).Should().BeOfType<NotFoundResult>();
            (await controller.QuestionnairesForUser("nobody", "")).Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task when_user_found_by_email_should_return_user_model()
        {
            var user = new DesignerIdentityUser { Id = Guid.NewGuid(), UserName = "john", Email = "j@x.com" };
            users.Setup(x => x.FindByEmailAsync("j@x.com")).ReturnsAsync(user);
            users.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Administrator" });
            users.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            users.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(new List<Claim> { new Claim(ClaimTypes.Name, "John Doe") });

            var model = (await controller.GetUserInfo("j@x.com")).Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<PortalUserModel>().Subject;

            model.Id.Should().Be(user.Id);
            model.Login.Should().Be("john");
            model.Roles.Should().Equal("Administrator");
            model.FullName.Should().Be("John Doe");
        }

        [Test]
        public async Task when_user_not_found_by_email_should_fall_back_to_user_name()
        {
            var user = new DesignerIdentityUser { Id = Guid.NewGuid(), UserName = "john", Email = "j@x.com" };
            users.Setup(x => x.FindByEmailAsync("john")).ReturnsAsync((DesignerIdentityUser?)null);
            users.Setup(x => x.FindByNameAsync("john")).ReturnsAsync(user);
            users.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

            (await controller.GetUserInfo("john")).Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task when_listing_questionnaires_should_map_items_and_total()
        {
            var user = new DesignerIdentityUser { Id = Guid.NewGuid(), UserName = "john" };
            users.Setup(x => x.FindByNameAsync("john")).ReturnsAsync(user);
            var id = Guid.NewGuid();
            helper.Setup(x => x.GetQuestionnaires(user, false, QuestionnairesType.My | QuestionnairesType.Shared, null, 1, null, null, "flt"))
                .Returns(new[] { new QuestionnaireListViewModel { Id = id.ToString("N"), Title = "Q1" } }.ToPagedList(1, 20, 42));

            var package = (await controller.QuestionnairesForUser("john", "flt")).Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<PagedQuestionnaireCommunicationPackage>().Subject;

            package.TotalCount.Should().Be(42);
            var item = package.Items.Single();
            item.Id.Should().Be(id);
            item.Title.Should().Be("Q1");
        }
    }
}

