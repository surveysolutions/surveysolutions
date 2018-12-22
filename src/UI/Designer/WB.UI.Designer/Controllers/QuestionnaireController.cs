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
using CsvHelper.TypeConversion;
using Main.Core.Documents;
using System.Web.Routing;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
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
        public ActionResult DetailsNoSection(Guid id, Guid? chapterId, string entityType, Guid? entityid)
        {
            if (UserHelper.WebUser.IsAdmin || this.UserHasAccessToEditOrViewQuestionnaire(id))
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
                        false, sourceModel.Source);

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
                        isPublic: model.IsPublic,
                        variable: model.Variable);

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
            var optionsList = options.ToList();

            this.questionWithOptionsViewModel = new EditOptionsViewModel()
            {
                QuestionnaireId = id,
                QuestionId = questionId,
                QuestionTitle = editQuestionView.Title,
                Options = optionsList,
                SourceOptions = optionsList
            };

            if (editQuestionView.CascadeFromQuestionId != null)
            {
                var cascadingParentQuestionId = Guid.Parse(editQuestionView.CascadeFromQuestionId);
                var cascadingParentQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, cascadingParentQuestionId);
                this.questionWithOptionsViewModel.CascadingParent = new CascadingParent
                {
                    Values = cascadingParentQuestionView.Options.Select(x => (int) x.Value).ToHashSet(),
                    VariableName = cascadingParentQuestionView.VariableName
                };

                if (questionWithOptionsViewModel.CascadingParent.Values.Count == 0)
                {
                    this.Error(string.Format(Resources.QuestionnaireController.NoParentCascadingOptions,
                        this.questionWithOptionsViewModel.CascadingParent.VariableName));
                    return;
                }
            }
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
            this.GetOptionsFromStream(csvFile, false);

            return this.View(this.questionWithOptionsViewModel.Options);
        }

        [HttpPost]
        public ViewResult EditCascadingOptions(HttpPostedFileBase csvFile)
        {
            this.GetOptionsFromStream(csvFile, true);

            return this.View(this.questionWithOptionsViewModel.Options);
        }

        private void GetOptionsFromStream(HttpPostedFileBase csvFile, bool isCascading)
        {
            if (csvFile == null)
            {
                this.Error(Resources.QuestionnaireController.SelectTabFile);
                return;
            }

            if (isCascading && this.questionWithOptionsViewModel.CascadingParent?.Values.Count == 0)
            {
                this.Error(string.Format(Resources.QuestionnaireController.NoParentCascadingOptions,
                    this.questionWithOptionsViewModel.CascadingParent.VariableName));
                return;
            }

            try
            {
                var list = new List<QuestionnaireCategoricalOption>();
                var hasErrors = false;
                using (var csvReader = new CsvReader(new StreamReader(csvFile.InputStream), this.CreateCsvConfiguration(isCascading)))
                {
                    while (csvReader.Read())
                    {
                        try
                        {
                            list.Add(csvReader.GetRecord<QuestionnaireCategoricalOption>());
                        }
                        catch (Exception e)
                        {
                            hasErrors = true;
                            if (e.InnerException is CsvReaderException csvReaderException)
                                this.Error(
                                    $"({Resources.QuestionnaireController.Column}: {csvReaderException.ColumnIndex + 1}, " +
                                    $"{Resources.QuestionnaireController.Row}: {csvReaderException.RowIndex}) " +
                                    $"{csvReaderException.Message}", true);
                            else
                                this.Error(e.Message, true);
                        }
                    }
                        
                }

                if (!hasErrors)
                    this.questionWithOptionsViewModel.Options = list;
            }
            catch (Exception e)
            {
                this.Error(Resources.QuestionnaireController.TabFilesOnly);
                this.logger.Error(e.Message, e);
            }
        }

        private Configuration CreateCsvConfiguration(bool isCascading)
        {
            var cfg = new Configuration
            {
                HasHeaderRecord = false,
                TrimOptions = TrimOptions.Trim,
                IgnoreQuotes = false,
                Delimiter = "\t",
                // Skip if all fields are empty.
                ShouldSkipRecord = record => record.All(string.IsNullOrEmpty)

            };

            if (isCascading)
                cfg.RegisterClassMap(new CascadingOptionMap(this.questionWithOptionsViewModel.CascadingParent.Values));
            else
                cfg.RegisterClassMap<CategoricalOptionMap>();

            return cfg;
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

            return File(SaveOptionsToStream(this.questionWithOptionsViewModel.SourceOptions,
                this.questionWithOptionsViewModel.CascadingParent != null), "text/csv", fileDownloadName);
        }

        public class EditOptionsViewModel
        {
            public string QuestionnaireId { get; set; }
            public Guid QuestionId { get; set; }
            public List<QuestionnaireCategoricalOption> Options { get; set; }
            public List<QuestionnaireCategoricalOption> SourceOptions { get; set; }
            public string QuestionTitle { get; set; }
            public CascadingParent CascadingParent { get; set;}
        }

        public class CascadingParent
        {
            public HashSet<int> Values { get; set; }
            public string VariableName { get; set; }
        }

        private class CascadingOptionMap : CategoricalOptionMap
        {
            public CascadingOptionMap(HashSet<int> cascadingParentValues)
            {
                Map(m => m.ParentValue).Index(2).TypeConverter(new ConvertToInt32AndCheckParentOptionValueOrThrow(cascadingParentValues));
            }

            private class ConvertToInt32AndCheckParentOptionValueOrThrow : ConvertToInt32OrThrow
            {
                private readonly HashSet<int> cascadingParentValues;

                public ConvertToInt32AndCheckParentOptionValueOrThrow(HashSet<int> cascadingParentValues)
                {
                    this.cascadingParentValues = cascadingParentValues;
                }

                public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
                {
                    var convertedValue = (int)base.ConvertFromString(text, row, memberMapData);

                    if(!this.cascadingParentValues.Contains(convertedValue))
                        throw new CsvReaderException(row.Context.Row, memberMapData.Index, 
                            string.Format(Resources.QuestionnaireController.ImportOptions_ParentValueNotFound, convertedValue));

                    return convertedValue;
                }
            }
        }

        private class CategoricalOptionMap : ClassMap<QuestionnaireCategoricalOption>
        {
            public CategoricalOptionMap()
            {
                Map(m => m.Value).Index(0).TypeConverter<ConvertToInt32OrThrow>();
                Map(m => m.Title).Index(1).TypeConverter<ConvertToStringOrThrow>();
            }

            private class ConvertToStringOrThrow : DefaultTypeConverter
            {
                public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
                {
                    return !string.IsNullOrEmpty(text)
                        ? text
                        : throw new CsvReaderException(row.Context.Row, memberMapData.Index, Resources.QuestionnaireController.ImportOptions_EmptyValue);
                }
            }

            protected class ConvertToInt32OrThrow : DefaultTypeConverter
            {
                public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
                {
                    if(string.IsNullOrEmpty(text))
                        throw new CsvReaderException(row.Context.Row, memberMapData.Index, Resources.QuestionnaireController.ImportOptions_EmptyValue);

                    var numberStyle = memberMapData.TypeConverterOptions.NumberStyle ?? NumberStyles.Integer;

                    return int.TryParse(text, numberStyle, memberMapData.TypeConverterOptions.CultureInfo, out var i)
                        ? i
                        : throw new CsvReaderException(row.Context.Row, memberMapData.Index, 
                            string.Format(Resources.QuestionnaireController.ImportOptions_NotNumber, text));
                }
            }
        }
        
        private class CsvReaderException : Exception
        {
            public readonly int? RowIndex;
            public readonly int? ColumnIndex;

            public CsvReaderException(int? rowIndex, int? columnIndex, string message) : base(message)
            {
                this.RowIndex = rowIndex;
                this.ColumnIndex = columnIndex;
            }
        }

        private Stream SaveOptionsToStream(IEnumerable<QuestionnaireCategoricalOption> options, bool isCascading)
        {
            var sb = new StringBuilder();

            using (var csvWriter = new CsvWriter(new StringWriter(sb), this.CreateCsvConfiguration(isCascading)))
            {
                csvWriter.WriteRecords(options);
            }

            return new MemoryStream(Encoding.Unicode.GetBytes(sb.ToString()));
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
