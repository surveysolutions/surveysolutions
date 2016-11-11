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
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    public class QuestionnaireController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IQuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly ILookupTableService lookupTableService;
        private readonly IQuestionnaireInfoFactory questionnaireInfoFactory;
        private readonly ILogger logger;

        public QuestionnaireController(
            ICommandService commandService,
            IMembershipUserService userHelper,
            IQuestionnaireHelper questionnaireHelper,
            IQuestionnaireViewFactory questionnaireViewFactory,
            ILogger logger,
            IQuestionnaireInfoFactory questionnaireInfoFactory,
            IQuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory, 
            ILookupTableService lookupTableService)
            : base(userHelper)
        {
            this.commandService = commandService;
            this.questionnaireHelper = questionnaireHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.logger = logger;
            this.questionnaireInfoFactory = questionnaireInfoFactory;
            this.questionnaireChangeHistoryFactory = questionnaireChangeHistoryFactory;
            this.lookupTableService = lookupTableService;
        }

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
        [PreventDoubleSubmit]
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
                    this.Error("You don't  have permissions to delete this questionnaire.");
                }
                else
                {
                    var command = new DeleteQuestionnaire(model.PublicKey, UserHelper.WebUser.UserId);
                    this.commandService.Execute(command);

                    this.Success($"Questionnaire \"{model.Title}\" successfully deleted");
                }
            }
            return this.Redirect(this.Request.UrlReferrer.ToString());
        }

        [HttpPost]
        public ActionResult Revert(Guid id, Guid commandId)
        {
            // TODO: KP-8105 add command here
            string sid = id.FormatGuid();
            return RedirectToAction("Details", new { id = sid });
        }

        [AllowAnonymous]
        public ActionResult ExpressionGeneration(Guid? id)
        {
            ViewBag.QuestionnaireId = id ?? Guid.Empty;
            return this.View();
        }

        public ActionResult Index(int? p, string sb, int? so, string f)
        {
            return this.View(this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, filter: f));
        }

        public ActionResult Public(int? p, string sb, int? so, string f)
        {
            var questionnairePublicListViewModels = this.GetPublicQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, filter: f);
            return this.View(questionnairePublicListViewModels);
        }

        public ActionResult QuestionnaireHistory(Guid id, int? page)
        {
            QuestionnaireChangeHistory questionnairePublicListViewModels = questionnaireChangeHistoryFactory.Load(id, page ?? 1, GlobalHelper.GridPageItemsCount);

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

            var options = editQuestionView != null
                ? editQuestionView.Options.Select(
                    option => new Option(Guid.NewGuid(), option.Value.ToString("G29",CultureInfo.InvariantCulture), option.Title, option.ParentValue))
                : new Option[0];

            this.questionWithOptionsViewModel = new EditOptionsViewModel()
            {
                QuestionnaireId = id,
                QuestionId = questionId,
                QuestionTitle = editQuestionView.Title,
                Options = options,
                SourceOptions = options
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

        private CsvConfiguration CreateCsvConfiguration()
        {
            return new CsvConfiguration { HasHeaderRecord = false, TrimFields = true, IgnoreQuotes = false, Delimiter = "\t" };
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
            return
                File(SaveOptionsToStream(this.questionWithOptionsViewModel.SourceOptions), "text/csv",
                    string.Format("Options-in-question-{0}.txt",
                        this.questionWithOptionsViewModel.QuestionTitle.Length > 50
                            ? this.questionWithOptionsViewModel.QuestionTitle.Substring(0, 50)
                            : this.questionWithOptionsViewModel.QuestionTitle));
        }

        public class EditOptionsViewModel
        {
            public string QuestionnaireId { get; set; }
            public Guid QuestionId { get; set; }
            public IEnumerable<Option> Options { get; set; }
            public IEnumerable<Option> SourceOptions { get; set; }
            public string QuestionTitle { get; set; }
        }

        private IEnumerable<Option> ExtractOptionsFromStream(Stream inputStream, bool isCascade)
        {
            var importedOptions = new List<Option>();

            var csvReader = new CsvReader(new StreamReader(inputStream), this.CreateCsvConfiguration());
            using (csvReader)
            {
                while (csvReader.Read())
                {
                    if (isCascade)
                    {
                        importedOptions.Add(new Option(Guid.NewGuid(), csvReader.GetField(0), csvReader.GetField(1), csvReader.GetField(2)));
                    }
                    else
                    {
                        importedOptions.Add(new Option(Guid.NewGuid(), csvReader.GetField(0), csvReader.GetField(1)));    
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

        private IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            int? pageIndex, string sortBy, int? sortOrder, string filter)
        {
            this.SaveRequest(pageIndex: pageIndex, sortBy: ref sortBy, sortOrder: sortOrder, filter: filter);

            return this.questionnaireHelper.GetPublicQuestionnaires(
                pageIndex: pageIndex,
                sortBy: sortBy,
                sortOrder: sortOrder,
                filter: filter,
                viewerId: UserHelper.WebUser.UserId,
                isAdmin: UserHelper.WebUser.IsAdmin);
        }

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

        private IPagedList<QuestionnaireListViewModel> GetQuestionnaires(int? pageIndex, string sortBy, int? sortOrder, string filter)
        {
            this.SaveRequest(pageIndex: pageIndex, sortBy: ref sortBy, sortOrder: sortOrder, filter: filter);

            return this.questionnaireHelper.GetQuestionnaires(
                pageIndex: pageIndex,
                sortBy: sortBy,
                sortOrder: sortOrder,
                filter: filter,
                viewerId: UserHelper.WebUser.UserId,
                isAdmin: UserHelper.WebUser.IsAdmin);
        }
        
        private void SaveRequest(int? pageIndex, ref string sortBy, int? sortOrder, string filter)
        {
            this.ViewBag.PageIndex = pageIndex;
            this.ViewBag.SortBy = sortBy;
            this.ViewBag.Filter = filter;
            this.ViewBag.SortOrder = sortOrder;

            if (sortOrder.ToBool())
            {
                sortBy = $"{sortBy} Desc";
            }
        }

        public ActionResult LackOfPermits()
        {
            this.Error("You don't have permission to edit this questionnaire");
            return this.RedirectToAction("Index");
        }
    }
}