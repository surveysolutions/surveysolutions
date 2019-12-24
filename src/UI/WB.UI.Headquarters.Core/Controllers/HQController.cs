using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class HQController : Controller
    {
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private readonly ILogger<HQController> logger;
        private readonly IQuestionnaireExporter questionnaireExporter;
        private readonly EventBusSettings eventBusSettings;

        public HQController(ICommandService commandService,
            IAuthorizedUser authorizedUser,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IQuestionnaireVersionProvider questionnaireVersionProvider,
            IQuestionnaireStorage questionnaireStorage, 
            ILogger<HQController> logger,
            IQuestionnaireExporter questionnaireExporter,
            EventBusSettings eventBusSettings)
        {
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
            this.logger = logger;
            this.questionnaireExporter = questionnaireExporter;
            this.eventBusSettings = eventBusSettings;
        }

        public IActionResult Index()
        {
            return RedirectToActionPermanent("Index","Interviews");
        }

        public IActionResult Interviews(Guid? questionnaireId)
        {
            return RedirectToActionPermanent("Index", "Interviews");
        }
        
        [ObserverNotAllowed]
        [Authorize(Roles = "Administrator")]
        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult CloneQuestionnaire(Guid id, long version)
        {
            QuestionnaireBrowseItem questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version));

            if (questionnaireBrowseItem == null)
                return NotFound(string.Format(HQ.QuestionnaireNotFoundFormat, id.FormatGuid(), version));

            return this.View(new CloneQuestionnaireModel(id, version, questionnaireBrowseItem.Title, questionnaireBrowseItem.AllowCensusMode, questionnaireBrowseItem.Comment));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        [Authorize(Roles = "Administrator")]
        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult CloneQuestionnaire(CloneQuestionnaireModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }
            try
            {
                var newVersion = this.questionnaireVersionProvider.GetNextVersion(model.Id);
                this.commandService.Execute(new CloneQuestionnaire(
                    model.Id, model.Version, model.NewTitle, newQuestionnaireVersion:newVersion, userId: this.authorizedUser.Id, comment: model.Comment));
            }
            catch (QuestionnaireException exception)
            {
                this.ModelState.AddModelError<CloneQuestionnaireModel>(x => x.NewTitle, exception.Message);
                return this.View(model);
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Unexpected error occurred while cloning questionnaire (id: {model.Id}, version: {model.Version}).", exception);
                //this.Error(QuestionnaireClonning.UnexpectedError); todo KP-13494
                return this.View(model);
            }

            //this.Success( todo KP-13494
            //    model.NewTitle == model.OriginalTitle
            //        ? string.Format(HQ.QuestionnaireClonedFormat, model.OriginalTitle)
            //        : string.Format(HQ.QuestionnaireClonedAndRenamedFormat, model.OriginalTitle, model.NewTitle));

            return this.RedirectToAction("Index", "SurveySetup");
        }

        [ObserverNotAllowed]
        [Authorize(Roles = "Administrator")]
        public IActionResult ExportQuestionnaire(Guid id, long version)
        {
            var file = questionnaireExporter.CreateZipExportFile(new QuestionnaireIdentity(id, version));
            return File(file.FileStream, "application/zip", file.Filename);
        }

        public IActionResult TakeNew(string questionnaireId)
        {
            var newInterviewId = Guid.NewGuid();
            this.eventBusSettings.IgnoredAggregateRoots.Add(newInterviewId.FormatGuid());

            var identity = QuestionnaireIdentity.Parse(questionnaireId);
            var command = new CreateTemporaryInterviewCommand(newInterviewId, this.authorizedUser.Id, identity);

            try
            {
                this.commandService.Execute(command);
            }
            catch (InterviewException ie)
            {
                //Error(Assignments.ErrorToCreateAssignment + ie.Message); todo KP-13496
                return this.RedirectToAction("Index", "SurveySetup");
            }

            return this.RedirectToAction("TakeNewAssignment", new {id = newInterviewId.FormatGuid()});
        }

        public IActionResult TakeNewAssignment(string id)
        {
            if (!this.eventBusSettings.IsIgnoredAggregate(Guid.Parse(id)))
                return NotFound();

            return this.View(new
            {
                id = id,
                responsiblesUrl = Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "Teams", action = "ResponsiblesCombobox"}),
                createNewAssignmentUrl = Url.Content(@"~/api/Assignments/Create"),
                maxInterviewsByAssignment = Constants.MaxInterviewsCountByAssignment,
                assignmentsUrl = Url.Action("Index", "Assignments", new { id = (int?)null })
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
