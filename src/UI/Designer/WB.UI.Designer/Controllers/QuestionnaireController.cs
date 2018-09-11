using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Main.Core.Entities.SubEntities;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Controllers
{
    public class QuestionnaireController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IQuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILookupTableService lookupTableService;
        private readonly IQuestionnaireInfoFactory questionnaireInfoFactory;
        private readonly ILogger logger;
        private readonly IQuestionnaireInfoViewFactory questionnaireInfoViewFactory;
        private readonly IPublicFoldersStorage publicFoldersStorage;

        public QuestionnaireController(
            ICommandService commandService,
            IMembershipUserService userHelper,
            IQuestionnaireHelper questionnaireHelper,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IFileSystemAccessor fileSystemAccessor,
            ILogger logger,
            IQuestionnaireInfoFactory questionnaireInfoFactory,
            IQuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory, 
            ILookupTableService lookupTableService, 
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory,
            IPublicFoldersStorage publicFoldersStorage)
            : base(userHelper)
        {
            this.commandService = commandService;
            this.questionnaireHelper = questionnaireHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.questionnaireInfoFactory = questionnaireInfoFactory;
            this.questionnaireChangeHistoryFactory = questionnaireChangeHistoryFactory;
            this.lookupTableService = lookupTableService;
            this.questionnaireInfoViewFactory = questionnaireInfoViewFactory;
            this.publicFoldersStorage = publicFoldersStorage;
        }

        [NoCache]
        public ActionResult Details(Guid id, Guid? chapterId, string entityType, Guid? entityid)
        {
            return (UserHelper.WebUser.IsAdmin || this.UserHasAccessToEditOrViewQuestionnaire(id)) 
                ? this.View("~/questionnaire/details/index.cshtml") 
                : this.LackOfPermits();
        }

        private bool UserHasAccessToEditOrViewQuestionnaire(Guid id)
        {
            return this.questionnaireViewFactory.HasUserAccessToQuestionnaire(id, this.UserHelper.WebUser.UserId);
        }

        public ActionResult Clone(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaireOrThrow404(id);
            return
                this.View(
                    new QuestionnaireCloneModel { Title = $"Copy of {model.Title}", Id = model.PublicKey });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Clone(QuestionnaireCloneModel model)
        {
            if (this.ModelState.IsValid)
            {
                QuestionnaireView sourceModel = this.GetQuestionnaireOrThrow404(model.Id);
                if (sourceModel == null)
                {
                    throw new ArgumentNullException(nameof(model));
                }
                try
                {
                    var questionnaireId = Guid.NewGuid();

                    var command = new CloneQuestionnaire(questionnaireId, model.Title, this.UserHelper.WebUser.UserId,
                        model.IsPublic, sourceModel.Source);

                    this.commandService.Execute(command);

                    return this.RedirectToAction("Details", "Questionnaire", new { id = questionnaireId.FormatGuid() });
                }
                catch (Exception e)
                {
                    logger.Error("Error on questionnaire cloning.", e);

                    var domainException = e.GetSelfOrInnerAs<QuestionnaireException>();
                    if (domainException != null)
                    {
                        this.Error(domainException.Message);
                        logger.Error("Questionnaire controller -> clone: " + domainException.Message, domainException);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return this.View(model);
        }

        public ActionResult Create()
        {
            return this.View(new QuestionnaireViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QuestionnaireViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var questionnaireId = Guid.NewGuid();

                try
                {
                    var command = new CreateQuestionnaire(
                        questionnaireId: questionnaireId,
                        text: model.Title,
                        responsibleId: this.UserHelper.WebUser.UserId,
                        isPublic: model.IsPublic);

                    this.commandService.Execute(command);

                    return this.RedirectToAction("Details", "Questionnaire", new {id = questionnaireId.FormatGuid()});
                }
                catch (QuestionnaireException e)
                {
                    Error(e.Message);
                    logger.Error("Error on questionnaire creation.", e);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);

            if (model != null)
            {
                if ((model.CreatedBy != UserHelper.WebUser.UserId) && !UserHelper.WebUser.IsAdmin)
                {
                    this.Error(Resources.QuestionnaireController.ForbiddenDelete);
                }
                else
                {
                    var command = new DeleteQuestionnaire(model.PublicKey, UserHelper.WebUser.UserId);
                    this.commandService.Execute(command);

                    this.Success(string.Format(Resources.QuestionnaireController.SuccessDeleteMessage, model.Title));
                }
            }
            return this.Redirect(this.Request.UrlReferrer.ToString());
        }

        [HttpPost]
        public ActionResult Revert(Guid id, Guid commandId)
        {
            var historyReferenceId = commandId;

            bool hasAccess = this.UserHelper.WebUser.IsAdmin || this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id, this.UserHelper.WebUser.UserId);
            if (!hasAccess)
            {
                this.Error(Resources.QuestionnaireController.ForbiddenRevert);
                return this.RedirectToAction("Index");
            }

            var command = new RevertVersionQuestionnaire(id, historyReferenceId, this.UserHelper.WebUser.UserId);
            this.commandService.Execute(command);

            string sid = id.FormatGuid();
            return this.RedirectToAction("Details", new {id =  sid});
        }

        [AllowAnonymous]
        public ActionResult ExpressionGeneration(Guid? id)
        {
            ViewBag.QuestionnaireId = id ?? Guid.Empty;
            return this.View();
        }

        [ValidateInput(false)]
        public ActionResult Index(int? p, string sb, int? so, string f) 
            => this.View(this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f, type: QuestionnairesType.My, folderId: null));

        [ValidateInput(false)]
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
                IsSupportAssignFolders = UserHelper.WebUser.IsAdmin,
                CurrentFolderId = id,
                Breadcrumbs = breadcrumbs,
                Questionnaires = questionnaires
            };

            return this.View(model);
        }

        [ValidateInput(false)]
        public ActionResult Shared(int? p, string sb, int? so, string f)
            => this.View(this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f, type: QuestionnairesType.Shared, folderId: null));

        [ValidateInput(false)]
        public ActionResult PublicFolders() 
            => this.View();

        public ActionResult QuestionnaireHistory(Guid id, int? page)
        {
            bool hasAccess = this.UserHelper.WebUser.IsAdmin || this.questionnaireViewFactory.HasUserAccessToQuestionnaire(id, this.UserHelper.WebUser.UserId);
            if (!hasAccess)
            {
                this.Error(ErrorMessages.NoAccessToQuestionnaire);
                return this.RedirectToAction("Index");
            }
            var questionnaireInfoView = this.questionnaireInfoViewFactory.Load(id.FormatGuid(), this.UserHelper.WebUser.UserId);

            QuestionnaireChangeHistory questionnairePublicListViewModels = questionnaireChangeHistoryFactory.Load(id, page ?? 1, GlobalHelper.GridPageItemsCount);
            questionnairePublicListViewModels.ReadonlyMode = questionnaireInfoView.IsReadOnlyForUser;

            return this.View(questionnairePublicListViewModels);
        }

        #region [Edit options]
        private const string OptionsSessionParameterName = "options";

        private EditOptionsViewModel questionWithOptionsViewModel
        {
            get { return (EditOptionsViewModel) this.Session[OptionsSessionParameterName]; }
            set { this.Session[OptionsSessionParameterName] = value; }
        }

        public ActionResult EditOptions(string id, Guid questionId)
        {
            this.SetupViewModel(id, questionId);
            return this.View(this.questionWithOptionsViewModel.Options);
        }

        public ActionResult EditCascadingOptions(string id, Guid questionId)
        {
            this.SetupViewModel(id, questionId);
            return this.View(this.questionWithOptionsViewModel.Options);
        }

        private void SetupViewModel(string id, Guid questionId)
        {
            var editQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, questionId);

            var options = editQuestionView?.Options.Select(
                              option => new Option(option.Value?.ToString("G29",CultureInfo.InvariantCulture), option.Title, option.ParentValue)) ?? new Option[0];
            var optionsList = options.ToList();

            this.questionWithOptionsViewModel = new EditOptionsViewModel()
            {
                QuestionnaireId = id,
                QuestionId = questionId,
                QuestionTitle = editQuestionView.Title,
                Options = optionsList,
                SourceOptions = optionsList
            };
        }

        public ActionResult ResetOptions()
        {
            return RedirectToAction("EditOptions",
                new
                {
                    id = this.questionWithOptionsViewModel.QuestionnaireId,
                    questionId = this.questionWithOptionsViewModel.QuestionId
                });
        }

        public ActionResult ResetCascadingOptions()
        {
            return RedirectToAction("EditCascadingOptions",
                new
                {
                    id = this.questionWithOptionsViewModel.QuestionnaireId,
                    questionId = this.questionWithOptionsViewModel.QuestionId
                });
        }

        [HttpPost]
        public ViewResult EditOptions(HttpPostedFileBase csvFile)
        {
            this.GetOptionsFromStream(csvFile);

            return this.View(this.questionWithOptionsViewModel.Options);
        }

        [HttpPost]
        public ViewResult EditCascadingOptions(HttpPostedFileBase csvFile)
        {
            this.GetOptionsFromStream(csvFile, isCascade: true);

            return this.View(this.questionWithOptionsViewModel.Options);
        }

        private void GetOptionsFromStream(HttpPostedFileBase csvFile, bool isCascade = false)
        {
            try
            {
                this.questionWithOptionsViewModel.Options = this.ExtractOptionsFromStream(csvFile.InputStream, isCascade);
            }
            catch (Exception)
            {
                if (csvFile == null)
                {
                    this.Error(Resources.QuestionnaireController.SelectTabFile);
                }
                else
                {
                    this.Error(Resources.QuestionnaireController.TabFilesOnly);
                }
            }
        }

        private Configuration CreateCsvConfiguration()
        {
            return new Configuration
            {
                HasHeaderRecord = false,
                TrimOptions = TrimOptions.Trim,
                IgnoreQuotes = false,
                Delimiter = "\t",
            };
        }

        public JsonResult ApplyOptions()
        {
            var commandResult = this.ExecuteCommand(
                new UpdateFilteredComboboxOptions(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.UserHelper.WebUser.UserId,
                        this.questionWithOptionsViewModel.Options.ToArray()));

            return Json(commandResult);
        }

        public JsonResult ApplyCascadingOptions()
        {
            var commandResult = this.ExecuteCommand(
                new UpdateCascadingComboboxOptions(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.UserHelper.WebUser.UserId,
                        this.questionWithOptionsViewModel.Options.ToArray()));

            return Json(commandResult);
        }
        
        private JsonResponseResult ExecuteCommand(QuestionCommand command)
        {
            var commandResult = new JsonResponseResult() { IsSuccess = true };
            try
            {
                this.commandService.Execute(command);
            }
            catch (Exception e)
            {
                var domainEx = e.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx == null)
                {
                    this.logger.Error(string.Format("Error on command of type ({0}) handling ", command.GetType()), e);
                }

                commandResult = new JsonResponseResult
                {
                    IsSuccess = false,
                    HasPermissions = domainEx != null && (domainEx.ErrorType != DomainExceptionType.DoesNotHavePermissionsForEdit),
                    Error = domainEx != null ? domainEx.Message : "Something goes wrong"
                };
            }
            return commandResult;
        }

        public FileResult ExportLookupTable(Guid id, Guid lookupTableId)
        {
            var lookupTableContentFile = this.lookupTableService.GetLookupTableContentFile(id, lookupTableId);
            return File(lookupTableContentFile.Content, "text/csv", lookupTableContentFile.FileName);
        }

        public FileResult ExportOptions()
        {
            var title = this.questionWithOptionsViewModel.QuestionTitle ?? "";
            var fileDownloadName = this.fileSystemAccessor.MakeValidFileName($"Options-in-question-{title}.txt");
            return
                File(SaveOptionsToStream(this.questionWithOptionsViewModel.SourceOptions), "text/csv",
                    fileDownloadName);
        }

        public class EditOptionsViewModel
        {
            public string QuestionnaireId { get; set; }
            public Guid QuestionId { get; set; }
            public List<Option> Options { get; set; }
            public List<Option> SourceOptions { get; set; }
            public string QuestionTitle { get; set; }
        }

        private List<Option> ExtractOptionsFromStream(Stream inputStream, bool isCascade)
        {
            var importedOptions = new List<Option>();

            var csvReader = new CsvReader(new StreamReader(inputStream), this.CreateCsvConfiguration());
            using (csvReader)
            {
                while (csvReader.Read())
                {
                    if (isCascade)
                    {
                        importedOptions.Add(new Option(csvReader.GetField(0), csvReader.GetField(1), csvReader.GetField(2)));
                    }
                    else
                    {
                        importedOptions.Add(new Option(csvReader.GetField(0), csvReader.GetField(1)));    
                    }
                }
            }

            return importedOptions;
        }

        private Stream SaveOptionsToStream(IEnumerable<Option> options)
        {
            var sb = new StringBuilder();
            using (var csvWriter = new CsvWriter(new StringWriter(sb),this.CreateCsvConfiguration()))
            {
                foreach (var option in options)
                {
                    if (String.IsNullOrEmpty(option.ParentValue))
                    {
                        csvWriter.WriteRecord(new { key = option.Value, value = option.Title });
                    }
                    else
                    {
                        csvWriter.WriteRecord(new { key = option.Value, value = option.Title, parent = option.ParentValue });
                    }
                    csvWriter.NextRecord();
                }
            }

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(sb.ToString());
            streamWriter.Flush();
            memoryStream.Position = 0;

            return memoryStream;
        }

        #endregion

        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            return questionnaire;
        }

        private QuestionnaireView GetQuestionnaireOrThrow404(Guid id)
        {
            QuestionnaireView questionnaire = GetQuestionnaire(id);

            if (questionnaire == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, $"Questionnaire with id={id} cannot be found");
            }

            return questionnaire;
        }

        private IPagedList<QuestionnaireListViewModel> GetQuestionnaires(int? pageIndex, string sortBy, int? sortOrder, string searchFor, QuestionnairesType type, Guid? folderId)
        {
            this.SaveRequest(pageIndex: pageIndex, sortBy: ref sortBy, sortOrder: sortOrder, searchFor: searchFor, folderId: folderId);

            return this.questionnaireHelper.GetQuestionnaires(
                pageIndex: pageIndex,
                sortBy: sortBy,
                sortOrder: sortOrder,
                searchFor: searchFor,
                folderId: folderId,
                viewerId: UserHelper.WebUser.UserId,
                isAdmin: UserHelper.WebUser.IsAdmin,
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

        public ActionResult LackOfPermits()
        {
            this.Error(Resources.QuestionnaireController.Forbidden);
            return this.RedirectToAction("Index");
        }

        private class ComboItem
        {
            public string Name { get; set; }
            public Guid? Value { get; set; }
        }

        [HttpPost]
        public ActionResult GetLanguages(Guid id)
        {
            var questionnaire = GetQuestionnaireOrThrow404(id);
            var comboBoxItems =
                new ComboItem { Name = QuestionnaireHistoryResources.Translation_Original, Value = null }.ToEnumerable().Concat(
                    questionnaire.Source.Translations.Select(i => new ComboItem { Name = i.Name ?? Resources.QuestionnaireController.Untitled, Value = i.Id })
                );
            return this.Json(comboBoxItems);
        }

        [HttpPost]
        public ActionResult AssignFolder(Guid id, Guid folderId)
        {
            var questionnaire = GetQuestionnaireOrThrow404(id);

            return this.Redirect(this.Request.UrlReferrer.ToString());
        }
    }
}
