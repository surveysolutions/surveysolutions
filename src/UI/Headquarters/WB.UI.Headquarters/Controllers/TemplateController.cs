using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Resources;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Template;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [AuthorizeOr403(Roles = "Administrator, Headquarter")]
    public class TemplateController : BaseController
    {
        private readonly IRestService designerQuestionnaireApiRestService;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private readonly IQuestionnaireImportService importService;
        private readonly DesignerUserCredentials designerUserCredentials;
        private readonly IAllUsersAndQuestionnairesFactory questionnaires;
        private readonly IAssignmentsUpgradeService upgradeService;

        public TemplateController(ICommandService commandService, ILogger logger,
            IRestService designerQuestionnaireApiRestService, IQuestionnaireVersionProvider questionnaireVersionProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory, IQuestionnaireImportService importService,
            DesignerUserCredentials designerUserCredentials, IAllUsersAndQuestionnairesFactory questionnaires,
            IAssignmentsUpgradeService upgradeService)
            : base(commandService, logger)
        {
            this.designerQuestionnaireApiRestService = designerQuestionnaireApiRestService;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
            this.importService = importService;
            this.designerUserCredentials = designerUserCredentials;
            this.questionnaires = questionnaires;
            this.upgradeService = upgradeService;
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (AppSettings.Instance.AcceptUnsignedCertificate)
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    (self, certificate, chain, sslPolicyErrors) => true;
            }
        }

        public ActionResult Import()
        {
            if (this.designerUserCredentials.Get() == null)
            {
                return this.RedirectToAction("LoginToDesigner");
            }

            return this.View(new ImportQuestionnaireListModel { DesignerUserName = this.designerUserCredentials.Get().Login });
        }

      
        public async Task<ActionResult> ImportMode(Guid id)
        {
            if (this.designerUserCredentials.Get() == null)
            {
                Error(Resources.LoginToDesigner.SessionExpired);
                return this.RedirectToAction("LoginToDesigner");
            }

            var model = await this.GetImportModel(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Import(Guid id, ImportModel request, bool showResult = false)
        {
            if (this.designerUserCredentials.Get() == null)
            {
                Error(Resources.LoginToDesigner.SessionExpired);
                return this.RedirectToAction("LoginToDesigner");
            }

            var model = await this.GetImportModel(id);
            if (model.QuestionnaireInfo != null)
            {
                var result = await this.importService.Import(id, model.QuestionnaireInfo?.Name, false);
                model.ErrorMessage = result.ImportError;

                if (result.IsSuccess)
                {
                    if (request.ShouldMigrateAssignments)
                    {
                        dynamic migrateFrom = JObject.Parse(request.MigrateFrom);
                        long version = migrateFrom.version;
                        Guid questionnaireId = migrateFrom.templateId;

                        var processId = Guid.NewGuid();
                        var sourceQuestionnaireId = new QuestionnaireIdentity(questionnaireId, version);
                        this.upgradeService.EnqueueUpgrade(processId, sourceQuestionnaireId, result.Identity);
                        return RedirectToAction("UpgradeProgress", "SurveySetup", new {id = processId});
                    }

                    if (showResult)
                        return Json(result);
                    
                    return this.RedirectToAction("Index", "SurveySetup");
                }
            }

            return this.View("ImportMode", model);
        }

        private async Task<ImportModeModel> GetImportModel(Guid id)
        {
            ImportModeModel model = new ImportModeModel();
            try
            {
                var questionnaireInfo = await this.designerQuestionnaireApiRestService
                    .GetAsync<QuestionnaireInfo>(url: $"/api/hq/v3/questionnaires/info/{id}",
                        credentials: this.designerUserCredentials.Get());

                model.QuestionnaireInfo = questionnaireInfo;
                model.NewVersionNumber = this.questionnaireVersionProvider.GetNextVersion(id);
                model.QuestionnairesToUpgradeFrom =
                    this.questionnaires.GetOlderQuestionnairesWithPendingAssignments(id, model.NewVersionNumber);

            }
            catch (RestException e)
            {
                switch (e.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        model.ErrorMessage = string.Format(ImportQuestionnaire.QuestionnaireCannotBeFound);
                        break;
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.Unauthorized:
                        this.designerUserCredentials.Set(null);
                        model.ErrorMessage = e.Message;
                        break;
                    default:
                        model.ErrorMessage = Strings.UnexpectedErrorOccurred;
                        break;
                }
            }

            return model;
        }

        public ActionResult LogoutFromDesigner()
        {
            this.designerUserCredentials.Set(null);
            return this.RedirectToAction("LoginToDesigner");
        }


        public ActionResult LoginToDesigner()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginToDesigner(LogOnModel model)
        {
            var designerUserCredentials = new RestCredentials { Login = model.UserName, Password = model.Password };

            try
            {
                await this.designerQuestionnaireApiRestService.GetAsync(url: @"/api/hq/user/login",
                    credentials: designerUserCredentials);

                this.designerUserCredentials.Set(designerUserCredentials);

                return this.RedirectToAction("Import");
            }
            catch (RestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                this.ModelState.AddModelError("InvalidCredentials", string.Empty);
            }
            catch (RestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                var position = ex.Message.IndexOf("IP:", StringComparison.InvariantCultureIgnoreCase);
                string ipString = position > -1 ? ex.Message.Substring(position) : "";

                this.ModelState.AddModelError("AccessForbidden", $"{Resources.LoginToDesigner.AccessForbidden} {ipString}");
            }
            catch (RestException ex)
            {
                this.Logger.Warn("Error communicating to designer", ex);
                this.Error(string.Format(
                    QuestionnaireImport.LoginToDesignerError,
                    GlobalHelper.GenerateUrl("Import", "Template", new { area = string.Empty })));
            }
            catch (Exception ex)
            {
                this.Logger.Error("Could not connect to designer.", ex);

                this.Error(string.Format(
                        QuestionnaireImport.LoginToDesignerError,
                        GlobalHelper.GenerateUrl("Import", "Template", new { area = string.Empty })));
            }

            return this.View(model);
        }
    }

    public class ImportModel
    {
        public bool ShouldMigrateAssignments { get; set; }

        public string MigrateFrom { get; set; }
    }
}
