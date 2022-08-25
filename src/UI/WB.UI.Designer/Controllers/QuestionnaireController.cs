using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SkiaSharp;
using SkiaSharp.QrCode.Image;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Filters;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Controllers
{
    [AuthorizeOrAnonymousQuestionnaire]
    [ResponseCache(NoStore = true)]
    [QuestionnairePermissions]
    public partial class QuestionnaireController : Controller
    {
        public class QuestionnaireCloneModel
        {
            [Key]
            public Guid QuestionnaireId { get; set; }

            public Guid? Revision { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireTitle_required))]
            [StringLength(AbstractVerifier.MaxTitleLength, ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireTitle_MaxLength), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
            public string? Title { get; set; }
        }

        public class QuestionnaireViewModel
        {
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "QuestionnaireTitle_required")]
            [StringLength(AbstractVerifier.MaxTitleLength, ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireTitle_MaxLength), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
            public string? Title { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "QuestionnaireVariable_required")]
            [RegularExpression(AbstractVerifier.VariableRegularExpression, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireVariable_rules))]
            [StringLength(AbstractVerifier.DefaultVariableLengthLimit, ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireVariable_MaxLength), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
            public string? Variable { get; set; }

            public bool IsPublic { get; set; }
        }

        private class ComboItem
        {
            public string? Name { get; set; }
            public Guid? Value { get; set; }
        }

        private readonly ICommandService commandService;
        private readonly IQuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILookupTableService lookupTableService;
        private readonly IQuestionnaireInfoFactory questionnaireInfoFactory;
        private readonly ILogger<QuestionnaireController> logger;
        private readonly IQuestionnaireInfoViewFactory questionnaireInfoViewFactory;
        private readonly ICategoricalOptionsImportService categoricalOptionsImportService;
        private readonly DesignerDbContext dbContext;
        private readonly ICategoriesService categoriesService;
        private readonly IEmailSender emailSender;
        private readonly IViewRenderService viewRenderService;
        private readonly UserManager<DesignerIdentityUser> users;
        private readonly IQuestionnaireHistoryVersionsService questionnaireHistoryVersionsService;

        public QuestionnaireController(
            IQuestionnaireViewFactory questionnaireViewFactory,
            IFileSystemAccessor fileSystemAccessor,
            ILogger<QuestionnaireController> logger,
            IQuestionnaireInfoFactory questionnaireInfoFactory,
            IQuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory,
            IQuestionnaireHistoryVersionsService questionnaireHistoryVersionsService,
            ILookupTableService lookupTableService,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory,
            ICategoricalOptionsImportService categoricalOptionsImportService,
            ICommandService commandService,
            DesignerDbContext dbContext,
            ICategoriesService categoriesService,
            IEmailSender emailSender,
            IViewRenderService viewRenderService,
            UserManager<DesignerIdentityUser> users)
        {
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.questionnaireInfoFactory = questionnaireInfoFactory;
            this.questionnaireChangeHistoryFactory = questionnaireChangeHistoryFactory;
            this.lookupTableService = lookupTableService;
            this.questionnaireInfoViewFactory = questionnaireInfoViewFactory;
            this.categoricalOptionsImportService = categoricalOptionsImportService;
            this.commandService = commandService;
            this.dbContext = dbContext;
            this.categoriesService = categoriesService;
            this.emailSender = emailSender;
            this.viewRenderService = viewRenderService;
            this.users = users;
            this.questionnaireHistoryVersionsService = questionnaireHistoryVersionsService;
        }

        [Route("questionnaire/details/{id}/nosection/{entityType}/{entityId}")]
        public IActionResult DetailsNoSection(QuestionnaireRevision id,
            Guid? chapterId, string entityType, Guid? entityid)
        {
            if (User.IsAdmin() || this.UserHasAccessToEditOrViewQuestionnaire(id.QuestionnaireId))
            {
                // get section id and redirect
                var sectionId = questionnaireInfoFactory.GetSectionIdForItem(id, entityid);
                return RedirectToActionPermanent("Details", new RouteValueDictionary
                {
                    { "id", id.ToString() }, {"chapterId", sectionId.FormatGuid()},{ "entityType", entityType},{ "entityid", entityid.FormatGuid()}
                });
            }

            return this.LackOfPermits();
        }

        [AntiForgeryFilter]
        [Route("questionnaire/details/{id}")]
        [Route("questionnaire/details/{id}/chapter/{chapterId}/{entityType}/{entityId}")]
        public IActionResult Details(QuestionnaireRevision? id, Guid? chapterId, string entityType, Guid? entityid)
        {
            if(id == null)
                return this.RedirectToAction("Index", "QuestionnaireList");

            var questionnaire = questionnaireViewFactory.Load(id);
            if (questionnaire == null || questionnaire.Source.IsDeleted)
                return NotFound();

            if (ShouldRedirectToOriginalId(id))
            {
                return RedirectToAction("Details", new RouteValueDictionary
                {
                    { "id", id.OriginalQuestionnaireId.FormatGuid() }, { "chapterId", chapterId?.FormatGuid() }, { "entityType", entityType }, { "entityid", entityid?.FormatGuid() }
                });
            }

            return (User.IsAdmin() || this.UserHasAccessToEditOrViewQuestionnaire(id.QuestionnaireId))
                ? this.View("~/questionnaire/index.cshtml")
                : this.LackOfPermits();
        }

        private bool ShouldRedirectToOriginalId(QuestionnaireRevision id)
        {
            if (!id.OriginalQuestionnaireId.HasValue || id.QuestionnaireId == id.OriginalQuestionnaireId)
                return false;

            if (User.Identity?.IsAuthenticated != true)
                return false;

            var userId = User.GetIdOrNull();
            if (!userId.HasValue)
                return false;

            if (User.IsAdmin())
                return true;

            var questionnaireId = id.OriginalQuestionnaireId.Value;
            var questionnaireListItem = this.dbContext.Questionnaires
                .Where(x => x.QuestionnaireId == questionnaireId.FormatGuid())
                .Include(x => x.SharedPersons).FirstOrDefault();

            if (questionnaireListItem == null)
                return false;

            if (questionnaireListItem.OwnerId == userId)
                return true;

            if (questionnaireListItem.IsPublic)
                return true;

            if (questionnaireListItem.SharedPersons.Any(x => x.UserId == userId))
                return true;

            return false;
        }

        private bool UserHasAccessToEditOrViewQuestionnaire(Guid id)
        {
            return this.questionnaireViewFactory.HasUserAccessToQuestionnaire(id, User.GetIdOrNull());
        }

        [Authorize]
        public IActionResult Clone(QuestionnaireRevision id)
        {
            var questionnaireId = id.OriginalQuestionnaireId ?? id.QuestionnaireId;
            QuestionnaireView? questionnaire = this.GetQuestionnaireView(questionnaireId);
            if (questionnaire == null) return NotFound();

            QuestionnaireView model = questionnaire;
            return View(
                    new QuestionnaireCloneModel
                    {
                        Title = $"Copy of {model.Title}",
                        QuestionnaireId = questionnaireId,
                        Revision = id.Revision
                    });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clone(QuestionnaireCloneModel model)
        {
            if (this.ModelState.IsValid)
            {
                QuestionnaireView? questionnaire =
                    this.questionnaireViewFactory.Load(new QuestionnaireRevision(model.QuestionnaireId, model.Revision));

                if (questionnaire == null)
                    return NotFound();

                QuestionnaireView sourceModel = questionnaire;
                try
                {
                    var questionnaireId = Guid.NewGuid();

                    var command = new CloneQuestionnaire(questionnaireId, model.Title ?? "", User.GetId(),
                        false, sourceModel.Source);

                    this.commandService.Execute(command);

                    await dbContext.SaveChangesAsync();

                    return this.RedirectToAction("Details", "Questionnaire", new { id = questionnaireId.FormatGuid() });
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error on questionnaire cloning.");

                    var domainException = e.GetSelfOrInnerAs<QuestionnaireException>();
                    if (domainException != null)
                    {
                        this.Error(domainException.Message);
                        logger.LogError(domainException, "Questionnaire controller -> clone: " + domainException.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return this.View(model);
        }

        [Authorize]
        public IActionResult Create()
        {
            return this.View(new QuestionnaireViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuestionnaireViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var questionnaireId = Guid.NewGuid();

                try
                {
                    var command = new CreateQuestionnaire(
                        questionnaireId: questionnaireId,
                        text: model.Title ?? "",
                        responsibleId: User.GetId(),
                        isPublic: model.IsPublic,
                        variable: model.Variable ?? "");

                    this.commandService.Execute(command);
                    this.dbContext.SaveChanges();

                    return this.RedirectToAction("Details", "Questionnaire", new { id = questionnaireId.FormatGuid() });
                }
                catch (QuestionnaireException e)
                {
                    this.Error(e.Message);
                    logger.LogError(e, "Error on questionnaire creation.");
                }
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            QuestionnaireView? model = this.GetQuestionnaireView(id);
            if (model != null)
            {
                if ((model.CreatedBy != User.GetId()) && !User.IsAdmin())
                {
                    this.Error(Resources.QuestionnaireController.ForbiddenDelete);
                }
                else
                {
                    var command = new DeleteQuestionnaire(model.PublicKey, User.GetId());
                    this.commandService.Execute(command);

                    this.Success(string.Format(Resources.QuestionnaireController.SuccessDeleteMessage, model.Title));
                }
            }
            return this.RedirectToAction("Index", "QuestionnaireList");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Revert(Guid id, Guid commandId)
        {
            var historyReferenceId = commandId;

            bool hasAccess = this.User.IsAdmin() || this.questionnaireViewFactory.HasUserChangeAccessToQuestionnaire(id, this.User.GetId());
            if (!hasAccess)
            {
                this.Error(Resources.QuestionnaireController.ForbiddenRevert);
                return this.RedirectToAction("Index", "QuestionnaireList");
            }

            var command = new RevertVersionQuestionnaire(id, historyReferenceId, this.User.GetId());
            this.commandService.Execute(command);

            string sid = id.FormatGuid();
            return this.RedirectToAction("Details", new { id = sid });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<bool>> SaveComment(Guid id, Guid historyItemId, string comment)
        {
            bool hasAccess = this.User.IsAdmin() 
                || this.questionnaireViewFactory.HasUserChangeAccessToQuestionnaire(id, this.User.GetId());

            if (!hasAccess)
                return false;

            var canEdit = this.questionnaireViewFactory.HasUserAccessToEditComments(historyItemId, this.User.GetId());

            if (!canEdit) return false;

            return await this.questionnaireHistoryVersionsService.UpdateRevisionCommentaryAsync(
                historyItemId.FormatGuid(), comment);
        }

        [Authorize]
        [AntiForgeryFilter]
        public async Task<IActionResult> QuestionnaireHistory(QuestionnaireRevision id, int? p)
        {
            bool hasAccess = this.User.IsAdmin() || this.questionnaireViewFactory.HasUserAccessToQuestionnaire(id.QuestionnaireId, this.User.GetIdOrNull());
            if (!hasAccess)
            {
                this.Error(ErrorMessages.NoAccessToQuestionnaire);
                return this.RedirectToAction("Index", "QuestionnaireList");
            }
            var questionnaireInfoView = this.questionnaireInfoViewFactory.Load(id, this.User.GetIdOrNull());
            if (questionnaireInfoView == null) return NotFound();

            QuestionnaireChangeHistory? questionnairePublicListViewModels = 
                await questionnaireChangeHistoryFactory.LoadAsync(id.QuestionnaireId, p ?? 1, GlobalHelper.GridPageItemsCount, this.User);
            if (questionnairePublicListViewModels == null)
                return NotFound();

            questionnairePublicListViewModels.ReadonlyMode = questionnaireInfoView.IsReadOnlyForUser;

            return this.View(questionnairePublicListViewModels);
        }

        [Authorize]
        public IActionResult ExpressionGeneration(Guid? id)
        {
            ViewBag.QuestionnaireId = id ?? Guid.Empty;
            return this.View();
        }

        private QuestionnaireView? GetQuestionnaireView(Guid id)
        {
            QuestionnaireView? questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            return questionnaire;
        }

        [Authorize]
        public IActionResult LackOfPermits()
        {
            this.Error(Resources.QuestionnaireController.Forbidden);
            return this.RedirectToAction("Index", "QuestionnaireList");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetLanguages(QuestionnaireRevision id)
        {
            QuestionnaireView? questionnaire = this.questionnaireViewFactory.Load(id);
            if (questionnaire == null) return NotFound();

            var comboBoxItems =
                new ComboItem
                {
                    Name = !string.IsNullOrEmpty(questionnaire.Source.DefaultLanguageName) ?
                        questionnaire.Source.DefaultLanguageName :
                        QuestionnaireHistoryResources.Translation_Original,
                    Value = null
                }.ToEnumerable()
                .Concat(
                    questionnaire.Source.Translations.Select(
                        i => new ComboItem { Name = i.Name ?? Resources.QuestionnaireController.Untitled, Value = i.Id })
                );
            return this.Json(comboBoxItems);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignFolder(Guid id, Guid folderId)
        {
            QuestionnaireView? questionnaire = GetQuestionnaireView(id);
            if (questionnaire == null)
                return NotFound();

            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer))
            {
                return this.Redirect(referer);
            }
            
            return Redirect(Url.Content("~/"));
        }

        /*[HttpGet]
        [Authorize(Roles = "Administrator")]
        public FileResult? Backup(Guid id)
        {
            var stream = this.questionnaireBackupService.GetBackupQuestionnaire(id, out string questionnaireFileName);
            
            return stream == null
                    ? null
                    : File(stream, "application/zip", $"{questionnaireFileName}.zip");
            
        }*/

        public class UpdateAnonymousQuestionnaireSettingsModel
        {
            public bool IsActive { get; set; }
        }
        
        [Authorize]
        [HttpPost]
        [Route("questionnaire/updateAnonymousQuestionnaireSettings/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAnonymousQuestionnaireSettings(Guid id, [FromBody] UpdateAnonymousQuestionnaireSettingsModel postModel)
        {
            bool isActive = postModel.IsActive;
            var anonymousQuestionnaire = dbContext.AnonymousQuestionnaires.FirstOrDefault(a => a.QuestionnaireId == id);
            if (anonymousQuestionnaire == null)
            {
                anonymousQuestionnaire = new AnonymousQuestionnaire()
                    { QuestionnaireId = id, AnonymousQuestionnaireId = Guid.NewGuid(), IsActive = isActive, GeneratedAtUtc = DateTime.UtcNow };
                dbContext.AnonymousQuestionnaires.Add(anonymousQuestionnaire);
                await dbContext.SaveChangesAsync();
            }

            anonymousQuestionnaire.IsActive = isActive;
            dbContext.AnonymousQuestionnaires.Update(anonymousQuestionnaire);
            await dbContext.SaveChangesAsync();

            if (isActive)
                await SendAnonymousSharingEmailAsync(id, anonymousQuestionnaire.AnonymousQuestionnaireId);
            
            return Json(new
            {
                AnonymousQuestionnaireId = anonymousQuestionnaire.AnonymousQuestionnaireId, 
                IsActive = isActive,
                GeneratedAtUtc = anonymousQuestionnaire.GeneratedAtUtc,
            });
        }

        [Authorize]
        [HttpPost]
        [Route("questionnaire/regenerateAnonymousQuestionnaireLink/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateAnonymousQuestionnaireLink(Guid id)
        {
            var existedRecord = dbContext.AnonymousQuestionnaires.First(a => a.QuestionnaireId == id);
            dbContext.Remove(existedRecord);
           
            var anonymousQuestionnaire = new AnonymousQuestionnaire()
                { QuestionnaireId = id, AnonymousQuestionnaireId = Guid.NewGuid(), IsActive = true, GeneratedAtUtc = DateTime.UtcNow };
            await dbContext.AnonymousQuestionnaires.AddAsync(anonymousQuestionnaire);
            await dbContext.SaveChangesAsync();

            await SendAnonymousSharingEmailAsync(id, anonymousQuestionnaire.AnonymousQuestionnaireId);
            
            return Json(new
            {
                AnonymousQuestionnaireId = anonymousQuestionnaire.AnonymousQuestionnaireId, 
                IsActive = true,
                GeneratedAtUtc = anonymousQuestionnaire.GeneratedAtUtc,
            });
        }

        private async Task SendAnonymousSharingEmailAsync(Guid id, Guid anonymousQuestionnaireId)
        {
            var questionnaireView = GetQuestionnaireView(id);
            if (questionnaireView == null)
                throw new ArgumentException($"Questionnaire not found {id}");
            
            var userName = User.GetUserName();
            var questionnaire = questionnaireView.Title;
            var sharingLink = Url.Action("Details", "Questionnaire", new { id = anonymousQuestionnaireId }, Request.Scheme);
            if (sharingLink == null)
                throw new ArgumentNullException("sharingLink is null");

            var model = new AnonymousSharingEmailModel(userName, sharingLink, questionnaire);

            var messageBody = await viewRenderService.RenderToStringAsync("Emails/AnonymousSharingEmail", model);

            var user = await this.users.GetUserAsync(User);
            await emailSender.SendEmailAsync(user.Email,
                NotificationResources.SystemMailer_AnonymousSharingEmail_Subject,
                messageBody);
        }

        [Authorize]
        [HttpGet]
        public IActionResult PublicUrl(Guid id, int height = 250, int width = 250)
        {
            var url = Url.Action("Details", "Questionnaire", new{ id = id }, Request.Scheme);
            
            // generate QRCode
            var qrCode = new QrCode(url, new Vector2Slim(height, width), SKEncodedImageFormat.Png);

            // output to file
            using var output = new MemoryStream();
            qrCode.GenerateImage(output);
            return File(output.ToArray(), "image/png", id.FormatGuid() + ".png",
                null, new EntityTagHeaderValue("\"" + id + "\""), false);
        }
    }
}
