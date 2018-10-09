using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.TakeNew;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [AuthorizeOr403(Roles = "Administrator, Headquarter")]
    public class HQController : BaseController
    {
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireExporter questionnaireExporter;
        private readonly EventBusSettings eventBusSettings;

        public HQController(ICommandService commandService,
            IAuthorizedUser authorizedUser,
            ILogger logger,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IQuestionnaireVersionProvider questionnaireVersionProvider,
            IQuestionnaireStorage questionnaireStorage, 
            IQuestionnaireExporter questionnaireExporter,
            EventBusSettings eventBusSettings)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireExporter = questionnaireExporter;
            this.eventBusSettings = eventBusSettings;
        }

        public ActionResult Index()
        {
            return RedirectToActionPermanent("Interviews");
        }

        public ActionResult Interviews(Guid? questionnaireId)
        {
            if (questionnaireId.HasValue)
            {
                this.Success(
                    $@"{HQ.InterviewWasCreated} <a class=""btn btn-success"" href=""{this.Url.Action("TakeNew", "HQ",
                        new { id = questionnaireId.Value })}""><i class=""icon-plus""></i>{HQ.CreateOneMore}</a>");
            }
            this.ViewBag.ActivePage = MenuItem.Docs;
            return this.View(this.Filters());
        }

        public ActionResult InterviewsRedesigned(Guid? questionnaireId)
        {
            if (questionnaireId.HasValue)
            {
                this.Success(
                    $@"{HQ.InterviewWasCreated} <a class=""btn btn-success"" href=""{this.Url.Action("TakeNew", "HQ",
                        new { id = questionnaireId.Value })}""><i class=""icon-plus""></i>{HQ.CreateOneMore}</a>");
            }
            this.ViewBag.ActivePage = MenuItem.Docs;
            return this.View(this.Filters());
        }

        [ObserverNotAllowed]
        [AuthorizeOr403(Roles = "Administrator")]
        public ActionResult CloneQuestionnaire(Guid id, long version)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            QuestionnaireBrowseItem questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version));

            if (questionnaireBrowseItem == null)
                return new HttpNotFoundResult(string.Format(HQ.QuestionnaireNotFoundFormat, id.FormatGuid(), version));

            return this.View(new CloneQuestionnaireModel(id, version, questionnaireBrowseItem.Title, questionnaireBrowseItem.AllowCensusMode));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        [AuthorizeOr403(Roles = "Administrator")]
        public ActionResult CloneQuestionnaire(CloneQuestionnaireModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }
            try
            {
                var newVersion = this.questionnaireVersionProvider.GetNextVersion(model.Id);
                this.CommandService.Execute(new CloneQuestionnaire(
                    model.Id, model.Version, model.NewTitle, newQuestionnaireVersion:newVersion, userId: this.authorizedUser.Id));
            }
            catch (QuestionnaireException exception)
            {
                this.ModelState.AddModelError<CloneQuestionnaireModel>(x => x.NewTitle, exception.Message);
                return this.View(model);
            }
            catch (Exception exception)
            {
                this.Logger.Error($"Unexpected error occurred while cloning questionnaire (id: {model.Id}, version: {model.Version}).", exception);
                this.Error(QuestionnaireClonning.UnexpectedError);
                return this.View(model);
            }

            this.Success(
                model.NewTitle == model.OriginalTitle
                    ? string.Format(HQ.QuestionnaireClonedFormat, model.OriginalTitle)
                    : string.Format(HQ.QuestionnaireClonedAndRenamedFormat, model.OriginalTitle, model.NewTitle));

            return this.RedirectToAction("Index", "SurveySetup");
        }

        [ObserverNotAllowed]
        [AuthorizeOr403(Roles = "Administrator")]
        public ActionResult ExportQuestionnaire(Guid id, long version)
        {
            var file = questionnaireExporter.CreateZipExportFile(new QuestionnaireIdentity(id, version));
            return File(file.FileStream, "application/zip", file.Filename);
        }

        public ActionResult TakeNew(string questionnaireId)
        {
            var newInterviewId = Guid.NewGuid();
            this.eventBusSettings.IgnoredAggregateRoots.Add(newInterviewId.FormatGuid());

            var identity = QuestionnaireIdentity.Parse(questionnaireId);
            var command = new CreateTemporaryInterviewCommand(newInterviewId, this.authorizedUser.Id, identity);

            this.CommandService.Execute(command);

            return this.RedirectToAction("TakeNewAssignment", new {id = newInterviewId.FormatGuid()});
        }

        public ActionResult TakeNewAssignment(string id)
        {
            return this.View(new
            {
                id = id,
                responsiblesUrl = Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "Teams", action = "ResponsiblesCombobox"})
            });
        }
        
        private DocumentFilter Filters()
        {
            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems(this.authorizedUser.IsSupervisor);

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load();

            return new DocumentFilter
            {
                Templates = usersAndQuestionnaires.Questionnaires,
                Statuses = statuses
            };
        }
    }
}
