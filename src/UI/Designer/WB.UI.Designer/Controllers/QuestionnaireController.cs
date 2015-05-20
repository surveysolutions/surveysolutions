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
using Resources;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize]
    public class QuestionnaireController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory;
        private readonly IQuestionnaireInfoFactory questionnaireInfoFactory;
        private readonly ILogger logger;

        public QuestionnaireController(
            ICommandService commandService,
            IMembershipUserService userHelper,
            IQuestionnaireHelper questionnaireHelper,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            ILogger logger,
            IQuestionnaireInfoFactory questionnaireInfoFactory,
            IExpressionProcessorGenerator expressionProcessorGenerator)
            : base(userHelper)
        {
            this.commandService = commandService;
            this.questionnaireHelper = questionnaireHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.logger = logger;
            this.questionnaireInfoFactory = questionnaireInfoFactory;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
        }

        public ActionResult Clone(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);
            return
                this.View(
                    new QuestionnaireCloneModel { Title = string.Format("Copy of {0}", model.Title), Id = model.PublicKey });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Clone(QuestionnaireCloneModel model)
        {
            if (this.ModelState.IsValid)
            {
                QuestionnaireView sourceModel = this.GetQuestionnaire(model.Id);
                if (sourceModel == null)
                {
                    throw new ArgumentNullException("model");
                }
                try
                {
                    var questionnaireId = Guid.NewGuid();
                    this.commandService.Execute(
                        new CloneQuestionnaireCommand(questionnaireId, model.Title, UserHelper.WebUser.UserId,
                            model.IsPublic, sourceModel.Source));

                    return this.RedirectToAction("Open", "App", new { id = questionnaireId });
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
                this.commandService.Execute(
                    new CreateQuestionnaireCommand(
                        questionnaireId: questionnaireId,
                        text: model.Title,
                        createdBy: UserHelper.WebUser.UserId,
                        isPublic: model.IsPublic));
                return this.RedirectToAction("Open", "App", new { id = questionnaireId });
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);
            if ((model.CreatedBy != UserHelper.WebUser.UserId) && !UserHelper.WebUser.IsAdmin)
            {
                this.Error("You don't  have permissions to delete this questionnaire.");
            }
            else
            {
                this.commandService.Execute(new DeleteQuestionnaireCommand(model.PublicKey));
                this.Success(string.Format("Questionnaire \"{0}\" successfully deleted", model.Title));
            }

            return this.Redirect(this.Request.UrlReferrer.ToString());
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
                new UpdateFilteredComboboxOptionsCommand(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.UserHelper.WebUser.UserId,
                        this.questionWithOptionsViewModel.Options.ToArray()));

            return Json(commandResult);
        }

        public JsonResult ApplyCascadingOptions()
        {
            var commandResult = this.ExecuteCommand(
                new UpdateCascadingComboboxOptionsCommand(
                        Guid.Parse(this.questionWithOptionsViewModel.QuestionnaireId),
                        this.questionWithOptionsViewModel.QuestionId,
                        this.UserHelper.WebUser.UserId,
                        this.questionWithOptionsViewModel.Options.ToArray()));

            return Json(commandResult);
        }
        
        private JsonQuestionnaireResult ExecuteCommand(QuestionCommand command)
        {
            var commandResult = new JsonQuestionnaireResult() { IsSuccess = true };
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

                commandResult = new JsonQuestionnaireResult
                {
                    IsSuccess = false,
                    HasPermissions = domainEx != null && (domainEx.ErrorType != DomainExceptionType.DoesNotHavePermissionsForEdit),
                    Error = domainEx != null ? domainEx.Message : "Something goes wrong"
                };
            }
            return commandResult;
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
                viewerId: UserHelper.WebUser.UserId);
        }

        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire =
                this.questionnaireViewFactory.Load(
                    new QuestionnaireViewInputModel(id));

            if (questionnaire == null)
            {
                throw new HttpException(
                    (int)HttpStatusCode.NotFound, string.Format("Questionnaire with id={0} cannot be found", id));
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
                viewerId: UserHelper.WebUser.UserId);
        }
        
        private void SaveRequest(int? pageIndex, ref string sortBy, int? sortOrder, string filter)
        {
            this.ViewBag.PageIndex = pageIndex;
            this.ViewBag.SortBy = sortBy;
            this.ViewBag.Filter = filter;
            this.ViewBag.SortOrder = sortOrder;

            if (sortOrder.ToBool())
            {
                sortBy = string.Format("{0} Desc", sortBy);
            }
        }

        public ActionResult LackOfPermits()
        {
            this.Error("You no longer have permission to edit this questionnaire");
            return this.RedirectToAction("Index");
        }
    }
}