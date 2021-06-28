using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.ComponentModels;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class SurveySetupController : Controller
    {
        private readonly IPreloadingTemplateService preloadingTemplateService;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAssignmentsUpgradeService upgradeService;
        private readonly IAllUsersAndQuestionnairesFactory questionnairesFactory;
        private readonly IExportFileNameService exportFileNameService;
        private readonly IAuthorizedUser authorizedUser;

        public SurveySetupController(
            IPreloadingTemplateService preloadingTemplateService,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IFileSystemAccessor fileSystemAccessor,
            IAssignmentsUpgradeService upgradeService,
            IAllUsersAndQuestionnairesFactory questionnairesFactory, 
            IExportFileNameService exportFileNameService,
            IAuthorizedUser authorizedUser)
        {
            this.preloadingTemplateService = preloadingTemplateService;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.upgradeService = upgradeService;
            this.questionnairesFactory = questionnairesFactory;
            this.exportFileNameService = exportFileNameService;
            this.authorizedUser = authorizedUser;
        }

        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult Index()
        {
            var surveySetupModel = new SurveySetupModel();
            surveySetupModel.Title = Dashboard.Questionnaires;
            surveySetupModel.DataUrl = Url.Action("Questionnaires", "QuestionnairesApi");
            surveySetupModel.IsObserver = this.authorizedUser.IsObserving;
            surveySetupModel.IsAdmin = this.authorizedUser.IsAdministrator;
            surveySetupModel.QuestionnaireDetailsUrl = Url.Action("Details", "Questionnaires");
            surveySetupModel.TakeNewInterviewUrl = Url.Action("TakeNew", "HQ");
            surveySetupModel.BatchUploadUrl = Url.Action("Upload", "Assignments");
            surveySetupModel.MigrateAssignmentsUrl = Url.Action("UpgradeAssignments", "SurveySetup");
            surveySetupModel.WebInterviewUrl = Url.Action("Settings", "WebInterviewSetup");
            surveySetupModel.DownloadLinksUrl = Url.Action("Download", "LinksExport");
            surveySetupModel.CloneQuestionnaireUrl = Url.Action("Clone", "Questionnaires");
            surveySetupModel.ExportQuestionnaireUrl = Url.Action("ExportQuestionnaire", "HQ");
            surveySetupModel.SendInvitationsUrl = Url.Action("SendInvitations", "WebInterviewSetup");
            surveySetupModel.ImportQuestionnaireUrl = Url.Action("Import", "Template");

            return this.View(surveySetupModel);
        }

        [AntiForgeryFilter]
        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult UpgradeAssignments(Guid id, long version)
        {
            var model = new UpgradeAssignmentsModel();
            model.QuestionnaireIdentity = new QuestionnaireIdentity(id, version);
            model.SurveySetupUrl = Url.Action("Index");
            model.Questionnaires = 
                this.questionnairesFactory.GetOlderQuestionnairesWithPendingAssignments(id, version)
                .Select(x => 
                    new ComboboxOptionModel(new QuestionnaireIdentity(x.TemplateId, x.TemplateVersion).ToString(), 
                                            string.Format(Pages.QuestionnaireNameVersionFirst, x.TemplateName, x.TemplateVersion)))
                .ToList();
            return View(model);
        }

        [ActivePage(MenuItem.Questionnaires)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("UpgradeAssignments")]
        public async Task<IActionResult> UpgradeAssignmentsPost(Guid id, long version, CancellationToken token = default)
        {
            var processId = Guid.NewGuid();
            var sourceQuestionnaireId = QuestionnaireIdentity.Parse(Request.Form["sourceQuestionnaireId"]);
            await this.upgradeService.EnqueueUpgrade(processId, 
                authorizedUser.Id, sourceQuestionnaireId, new QuestionnaireIdentity(id, version), token);

            return RedirectToAction("UpgradeProgress", new {id = processId});
        }

        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult UpgradeProgress()
        {
            return View(new
            {
                ProgressUrl = Url.Action("Status", "AssignmentsUpgradeApi"),
                StopUrl = Url.Action("Stop", "AssignmentsUpgradeApi"),
                ExportErrorsUrl = Url.Action("ExportErrors", "AssignmentsUpgradeApi"),
                SurveySetupUrl = Url.Action("Index")
            });
        }

        public ActionResult TemplateDownload(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out QuestionnaireIdentity questionnaireIdentity))
                return NotFound(id);

            var pathToFile = this.preloadingTemplateService.GetFilePathToPreloadingTemplate(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            return this.File(this.fileSystemAccessor.ReadFile(pathToFile), "application/zip", fileDownloadName: this.fileSystemAccessor.GetFileName(pathToFile));
        }

        public IActionResult SimpleTemplateDownload(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out QuestionnaireIdentity questionnaireIdentity))
                return NotFound(id);

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireInfo == null || questionnaireInfo.IsDeleted)
                return NotFound();

            string fileName = exportFileNameService.GetFileNameForAssignmentTemplate(questionnaireIdentity);
            byte[] templateFile = this.preloadingTemplateService.GetPrefilledPreloadingTemplateFile(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            return this.File(templateFile, "text/tab-separated-values", fileDownloadName: fileName);
        }
    }
}
