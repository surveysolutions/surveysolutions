﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers
{
    [Authorize]
    [ResponseCache(NoStore = true)]
    public partial class QuestionnaireController : Controller
    {
        public class QuestionnaireCloneModel
        {
            [Key]
            public Guid Id { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireTitle_required))]
            [StringLength(AbstractVerifier.MaxTitleLength, ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireTitle_MaxLength), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
            public string Title { get; set; }
        }

        public class QuestionnaireViewModel
        {
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "QuestionnaireTitle_required")]
            [StringLength(AbstractVerifier.MaxTitleLength, ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireTitle_MaxLength), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
            public string Title { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "QuestionnaireVariable_required")]
            [RegularExpression(AbstractVerifier.VariableRegularExpression, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireVariable_rules))]
            [StringLength(AbstractVerifier.DefaultVariableLengthLimit, ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireVariable_MaxLength), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
            public string Variable { get; set; }

            public bool IsPublic { get; set; }
        }

        private class ComboItem
        {
            public string Name { get; set; }
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
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IPublicFoldersStorage publicFoldersStorage;
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationsService;

        public QuestionnaireController(
            IQuestionnaireViewFactory questionnaireViewFactory,
            IFileSystemAccessor fileSystemAccessor,
            ILogger<QuestionnaireController> logger,
            IQuestionnaireInfoFactory questionnaireInfoFactory,
            IQuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory, 
            ILookupTableService lookupTableService, 
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory,
            ICategoricalOptionsImportService categoricalOptionsImportService,
            ICommandService commandService,
            DesignerDbContext dbContext,
            IQuestionnaireHelper questionnaireHelper, 
            IPublicFoldersStorage publicFoldersStorage,
            IAttachmentService attachmentService,
            ITranslationsService translationsService)
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
            this.questionnaireHelper = questionnaireHelper;
            this.publicFoldersStorage = publicFoldersStorage;
            this.attachmentService = attachmentService;
            this.translationsService = translationsService;
        }

        
        [Route("questionnaire/details/{id}/nosection/{entityType}/{entityId}")]
        public IActionResult DetailsNoSection(Guid id, Guid? chapterId, string entityType, Guid? entityid)
        {
            if (User.IsAdmin() || this.UserHasAccessToEditOrViewQuestionnaire(id))
            {
                // get section id and redirect
                var sectionId = questionnaireInfoFactory.GetSectionIdForItem(id.FormatGuid(), entityid);
                return RedirectToActionPermanent("Details", new RouteValueDictionary
                {
                    { "id", id.FormatGuid() }, {"chapterId", sectionId.FormatGuid()},{ "entityType", entityType},{ "entityid", entityid.FormatGuid()}
                });
            }

            return this.LackOfPermits();
        }

        [Route("questionnaire/details/{id}")]
        [Route("questionnaire/details/{id}/chapter/{chapterId}/{entityType}/{entityId}")]
        public IActionResult Details(Guid id, Guid? chapterId, string entityType, Guid? entityid)
        {
            return (User.IsAdmin() || this.UserHasAccessToEditOrViewQuestionnaire(id)) 
                ? this.View("~/questionnaire/index.cshtml") 
                : this.LackOfPermits();
        }

        private bool UserHasAccessToEditOrViewQuestionnaire(Guid id)
        {
            return this.questionnaireViewFactory.HasUserAccessToQuestionnaire(id, User.GetId());
        }

        public IActionResult Clone(Guid id)
        {
            QuestionnaireView questionnaire = this.GetQuestionnaire(id);
            if (questionnaire == null) return NotFound();
            QuestionnaireView model = questionnaire;
            return View(
                    new QuestionnaireCloneModel
                    {
                        Title = $"Copy of {model.Title}",
                        Id = model.PublicKey
                    });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clone(QuestionnaireCloneModel model)
        {
            if (this.ModelState.IsValid)
            {
                QuestionnaireView questionnaire = this.GetQuestionnaire(model.Id);
                if (questionnaire == null)
                    return NotFound();
                QuestionnaireView sourceModel = questionnaire;
                try
                {
                    var questionnaireId = Guid.NewGuid();

                    var command = new CloneQuestionnaire(questionnaireId, model.Title, User.GetId(),
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

        public IActionResult Create()
        {
            return this.View(new QuestionnaireViewModel());
        }

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
                        text: model.Title,
                        responsibleId: User.GetId(),
                        isPublic: model.IsPublic,
                        variable: model.Variable);

                    this.commandService.Execute(command);
                    this.dbContext.SaveChanges();

                    return this.RedirectToAction("Details", "Questionnaire", new {id = questionnaireId.FormatGuid()});
                }
                catch (QuestionnaireException e)
                {
                    this.Error(e.Message);
                    logger.LogError(e, "Error on questionnaire creation.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);
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
            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Revert(Guid id, Guid commandId)
        {
            var historyReferenceId = commandId;

            bool hasAccess = this.User.IsAdmin() || this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id, this.User.GetId());
            if (!hasAccess)
            {
                this.Error(Resources.QuestionnaireController.ForbiddenRevert);
                return this.RedirectToAction("Index");
            }

            var command = new RevertVersionQuestionnaire(id, historyReferenceId, this.User.GetId());
            this.commandService.Execute(command);

            string sid = id.FormatGuid();
            return this.RedirectToAction("Details", new {id =  sid});
        }

        public IActionResult QuestionnaireHistory(Guid id, int? page)
        {
            bool hasAccess = this.User.IsAdmin() || this.questionnaireViewFactory.HasUserAccessToQuestionnaire(id, this.User.GetId());
            if (!hasAccess)
            {
                this.Error(ErrorMessages.NoAccessToQuestionnaire);
                return this.RedirectToAction("Index");
            }
            var questionnaireInfoView = this.questionnaireInfoViewFactory.Load(id.FormatGuid(), this.User.GetId());
            if (questionnaireInfoView == null) return NotFound();

            QuestionnaireChangeHistory questionnairePublicListViewModels = questionnaireChangeHistoryFactory.Load(id, page ?? 1, GlobalHelper.GridPageItemsCount);
            questionnairePublicListViewModels.ReadonlyMode = questionnaireInfoView.IsReadOnlyForUser;

            return this.View(questionnairePublicListViewModels);
        }

        public IActionResult ExpressionGeneration(Guid? id)
        {
            ViewBag.QuestionnaireId = id ?? Guid.Empty;
            return this.View();
        }

        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            return questionnaire;
        }

        public IActionResult LackOfPermits()
        {
            this.Error(Resources.QuestionnaireController.Forbidden);
            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult GetLanguages(Guid id)
        {
            QuestionnaireView questionnaire = GetQuestionnaire(id);
            if (questionnaire == null) return NotFound();

            var comboBoxItems =
                new ComboItem { Name = QuestionnaireHistoryResources.Translation_Original, Value = null }.ToEnumerable().Concat(
                    questionnaire.Source.Translations.Select(i => new ComboItem { Name = i.Name ?? Resources.QuestionnaireController.Untitled, Value = i.Id })
                );
            return this.Json(comboBoxItems);
        }

        [HttpPost]
        public IActionResult AssignFolder(Guid id, Guid folderId)
        {
            QuestionnaireView questionnaire = GetQuestionnaire(id);
            if (questionnaire == null)
                return NotFound();

            string referer = Request.Headers["Referer"].ToString();
            return this.Redirect(referer);
        }
    }
}
