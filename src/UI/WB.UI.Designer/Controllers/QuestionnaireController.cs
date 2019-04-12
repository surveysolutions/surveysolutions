using Main.Core.Entities.SubEntities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;
using WB.UI.Designer1.Extensions;

namespace WB.UI.Designer.Controllers
{
    [Authorize]
    [ResponseCache(NoStore = true)]
    public class QuestionnaireController : Controller
    {
        private readonly ICommandService commandService;
        private readonly IQuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILookupTableService lookupTableService;
        private readonly IQuestionnaireInfoFactory questionnaireInfoFactory;
        private readonly ILogger<QuestionnaireController> logger;
        private readonly IQuestionnaireInfoViewFactory questionnaireInfoViewFactory;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly ICategoricalOptionsImportService categoricalOptionsImportService;
        private readonly DesignerDbContext dbContext;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IPublicFoldersStorage publicFoldersStorage;

        public QuestionnaireController(
            IQuestionnaireViewFactory questionnaireViewFactory,
            IFileSystemAccessor fileSystemAccessor,
            ILogger<QuestionnaireController> logger,
            IQuestionnaireInfoFactory questionnaireInfoFactory,
            IQuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory, 
            ILookupTableService lookupTableService, 
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory,
            IHttpContextAccessor contextAccessor,
            ICategoricalOptionsImportService categoricalOptionsImportService,
            ICommandService commandService,
            DesignerDbContext dbContext,
            IQuestionnaireHelper questionnaireHelper, 
            IPublicFoldersStorage publicFoldersStorage)
        {
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.questionnaireInfoFactory = questionnaireInfoFactory;
            this.questionnaireChangeHistoryFactory = questionnaireChangeHistoryFactory;
            this.lookupTableService = lookupTableService;
            this.questionnaireInfoViewFactory = questionnaireInfoViewFactory;
            this.contextAccessor = contextAccessor;
            this.categoricalOptionsImportService = categoricalOptionsImportService;
            this.commandService = commandService;
            this.dbContext = dbContext;
            this.questionnaireHelper = questionnaireHelper;
            this.publicFoldersStorage = publicFoldersStorage;
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
            return this.questionnaireViewFactory.HasUserAccessToQuestionnaire(id, User.GetId().FormatGuid());
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
        public IActionResult Clone(QuestionnaireCloneModel model)
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
            return this.RedirectToAction("My", "Home");
        }

        [HttpPost]
        public IActionResult Revert(Guid id, Guid commandId)
        {
            var historyReferenceId = commandId;

            bool hasAccess = this.User.IsAdmin() || this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id, this.User.GetId().FormatGuid());
            if (!hasAccess)
            {
                this.Error(Resources.QuestionnaireController.ForbiddenRevert);
                return this.RedirectToAction("My", "Home");
            }

            var command = new RevertVersionQuestionnaire(id, historyReferenceId, this.User.GetId());
            this.commandService.Execute(command);

            string sid = id.FormatGuid();
            return this.RedirectToAction("Details", new {id =  sid});
        }

        [AllowAnonymous]
        public IActionResult ExpressionGeneration(Guid? id)
        {
            ViewBag.QuestionnaireId = id ?? Guid.Empty;
            return this.View();
        }

        public IActionResult QuestionnaireHistory(Guid id, int? page)
        {
            bool hasAccess = this.User.IsAdmin() || this.questionnaireViewFactory.HasUserAccessToQuestionnaire(id, this.User.GetId().FormatGuid());
            if (!hasAccess)
            {
                this.Error(ErrorMessages.NoAccessToQuestionnaire);
                return this.RedirectToAction("My");
            }
            var questionnaireInfoView = this.questionnaireInfoViewFactory.Load(id.FormatGuid(), this.User.GetId());

            QuestionnaireChangeHistory questionnairePublicListViewModels = questionnaireChangeHistoryFactory.Load(id, page ?? 1, GlobalHelper.GridPageItemsCount);
            questionnairePublicListViewModels.ReadonlyMode = questionnaireInfoView.IsReadOnlyForUser;

            return this.View(questionnairePublicListViewModels);
        }

        #region [Edit options]
        private const string OptionsSessionParameterName = "options";

        private EditOptionsViewModel questionWithOptionsViewModel
        {
            get
            {
                var model = this.contextAccessor.HttpContext.Session.Get(OptionsSessionParameterName);
                if (model != null)
                {
                    JsonConvert.DeserializeObject<EditOptionsViewModel>(Encoding.UTF8.GetString(model));
                }

                return null;
            }
            set
            {
                string data = JsonConvert.SerializeObject(value);

                this.contextAccessor.HttpContext.Session.Set(OptionsSessionParameterName, Encoding.UTF8.GetBytes(data));
            }
        }

        public IActionResult EditOptions(string id, Guid questionId)
        {
            this.SetupViewModel(id, questionId);
            return this.View(this.questionWithOptionsViewModel.Options);
        }

        public IActionResult EditCascadingOptions(string id, Guid questionId) 
            => this.EditOptions(id, questionId);

        private void SetupViewModel(string id, Guid questionId)
        {
            var editQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, questionId);

            var options = editQuestionView?.Options.Select(
                              option => new QuestionnaireCategoricalOption
                              {
                                  Value = (int)option.Value,
                                  ParentValue = (int?)option.ParentValue,
                                  Title = option.Title
                              }) ??
                          new QuestionnaireCategoricalOption[0];

            this.questionWithOptionsViewModel = new EditOptionsViewModel
            {
                QuestionnaireId = id,
                QuestionId = questionId,
                QuestionTitle = editQuestionView.Title,
                Options = options.ToArray()
            };
        }

        public IActionResult ResetOptions()
        {
            return RedirectToAction("EditOptions",
                new
                {
                    id = this.questionWithOptionsViewModel.QuestionnaireId,
                    questionId = this.questionWithOptionsViewModel.QuestionId
                });
        }

        public IActionResult ResetCascadingOptions()
        {
            return RedirectToAction("EditCascadingOptions",
                new
                {
                    id = this.questionWithOptionsViewModel.QuestionnaireId,
                    questionId = this.questionWithOptionsViewModel.QuestionId
                });
        }

        [HttpPost]
        public IActionResult EditOptions(IFormFile csvFile)
        {
            if (csvFile == null)
                this.Error(Resources.QuestionnaireController.SelectTabFile);
            else
            {
                try
                {
                    var importResult = this.categoricalOptionsImportService.ImportOptions(
                        csvFile.OpenReadStream(),
                        this.questionWithOptionsViewModel.QuestionnaireId,
                        this.questionWithOptionsViewModel.QuestionId);

                    if (importResult.Succeeded)
                        this.questionWithOptionsViewModel.Options = importResult.ImportedOptions.ToArray();
                    else
                    {
                        foreach (var importError in importResult.Errors)
                            this.Error(importError, true);
                    }
                }
                catch (Exception e)
                {
                    this.Error(Resources.QuestionnaireController.TabFilesOnly);
                    this.logger.LogError(e, e.Message);
                }
            }

            return this.View(this.questionWithOptionsViewModel.Options);
        }

        [HttpPost]
        public IActionResult EditCascadingOptions(IFormFile csvFile)
            => this.EditOptions(csvFile);

        public IActionResult ApplyOptions()
        {
            var commandResult = this.ExecuteCommand(
                new UpdateFilteredComboboxOptions(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.User.GetId(),
                        this.questionWithOptionsViewModel.Options.ToArray()));

            return Json(commandResult);
        }

        public IActionResult ApplyCascadingOptions()
        {
            var commandResult = this.ExecuteCommand(
                new UpdateCascadingComboboxOptions(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.User.GetId(),
                        this.questionWithOptionsViewModel.Options.ToArray()));

            return Json(commandResult);
        }
        
        private object ExecuteCommand(QuestionCommand command)
        {
            dynamic commandResult = new ExpandoObject();
            commandResult.IsSuccess = true;
            try
            {
                this.commandService.Execute(command);
            }
            catch (Exception e)
            {
                var domainEx = e.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx == null)
                {
                    this.logger.LogError(e, $"Error on command of type ({command.GetType()}) handling ");
                }

                commandResult = new ExpandoObject();
                commandResult.IsSuccess = false;
                commandResult.HasPermissions = domainEx != null && (domainEx.ErrorType != DomainExceptionType.DoesNotHavePermissionsForEdit);
                commandResult.Error = domainEx != null ? domainEx.Message : "Something goes wrong";
            }
            return commandResult;
        }

        public IActionResult ExportLookupTable(Guid id, Guid lookupTableId)
        {
            var lookupTableContentFile = this.lookupTableService.GetLookupTableContentFile(id, lookupTableId);
            return File(lookupTableContentFile.Content, "text/csv", lookupTableContentFile.FileName);
        }

        public IActionResult ExportOptions()
        {
            var title = this.questionWithOptionsViewModel.QuestionTitle ?? "";
            var fileDownloadName = this.fileSystemAccessor.MakeValidFileName($"Options-in-question-{title}.txt");

            return File(this.categoricalOptionsImportService.ExportOptions(
                this.questionWithOptionsViewModel.QuestionnaireId,
                this.questionWithOptionsViewModel.QuestionId), "text/csv", fileDownloadName);
        }

        public class EditOptionsViewModel
        {
            public string QuestionnaireId { get; set; }
            public Guid QuestionId { get; set; }
            public QuestionnaireCategoricalOption[] Options { get; set; }
            public string QuestionTitle { get; set; }
        }

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

        #endregion

        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            return questionnaire;
        }

        public IActionResult LackOfPermits()
        {
            this.Error(Resources.QuestionnaireController.Forbidden);
            return this.RedirectToAction("My");
        }

        private class ComboItem
        {
            public string Name { get; set; }
            public Guid? Value { get; set; }
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

        public ActionResult My(int? p, string sb, int? so, string f)
            => this.View(this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f, type: QuestionnairesType.My, folderId: null));

        public ActionResult Public(int? p, string sb, int? so, string f, Guid? id)
        {
            var questionnaires = this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f,
                type: QuestionnairesType.Public, folderId: id);

            var folderPath = publicFoldersStorage.GetFoldersPath(folderId: id);
            var breadcrumbs = folderPath.Select(folder => new FolderBreadcrumbsModel()
            {
                FolderId = folder.PublicId,
                Title = folder.Title
            }).ToArray();

            var model = new QuestionnaireListModel()
            {
                IsSupportAssignFolders = User.IsAdmin(),
                CurrentFolderId = id,
                Breadcrumbs = breadcrumbs,
                Questionnaires = questionnaires
            };

            return this.View(model);
        }

        public ActionResult Shared(int? p, string sb, int? so, string f)
            => this.View(this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f, type: QuestionnairesType.Shared, folderId: null));


        private IPagedList<QuestionnaireListViewModel> GetQuestionnaires(int? pageIndex, string sortBy, int? sortOrder, string searchFor, QuestionnairesType type, Guid? folderId)
        {
            this.SaveRequest(pageIndex: pageIndex, sortBy: ref sortBy, sortOrder: sortOrder, searchFor: searchFor, folderId: folderId);

            return this.questionnaireHelper.GetQuestionnaires(
                pageIndex: pageIndex,
                sortBy: sortBy,
                sortOrder: sortOrder,
                searchFor: searchFor,
                folderId: folderId,
                viewerId: User.GetId().FormatGuid(),
                isAdmin: User.IsAdmin(),
                type: type);
        }

        private void SaveRequest(int? pageIndex, ref string sortBy, int? sortOrder, string searchFor, Guid? folderId)
        {
            this.ViewBag.PageIndex = pageIndex;
            this.ViewBag.SortBy = sortBy;
            this.ViewBag.Filter = searchFor;
            this.ViewBag.SortOrder = sortOrder;
            this.ViewBag.FolderId = folderId;

            if (sortOrder.ToBool())
            {
                sortBy = $"{sortBy} Desc";
            }
        }
    }
}
