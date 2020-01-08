using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.Template;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ActivePage(MenuItem.Questionnaires)]
    public class TemplateController : Controller
    {
        private readonly IDesignerApi designerApi;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private readonly IQuestionnaireImportService importService;
        private readonly IDesignerUserCredentials designerUserCredentials;
        private readonly IAllUsersAndQuestionnairesFactory questionnaires;
        private readonly IAssignmentsUpgradeService upgradeService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ILogger<TemplateController> logger;

        public TemplateController(
            IDesignerApi designerApi, 
            IQuestionnaireVersionProvider questionnaireVersionProvider,
            IQuestionnaireImportService importService,
            IDesignerUserCredentials designerUserCredentials, 
            IAllUsersAndQuestionnairesFactory questionnaires,
            IAssignmentsUpgradeService upgradeService,
            IAuthorizedUser authorizedUser,
            ILogger<TemplateController> logger,
            IOptions<DesignerConfig> designerConfig)
        {
            this.designerApi = designerApi;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
            this.importService = importService;
            this.designerUserCredentials = designerUserCredentials;
            this.questionnaires = questionnaires;
            this.upgradeService = upgradeService;
            this.authorizedUser = authorizedUser;
            this.logger = logger;

            if (designerConfig.Value.AcceptUnsignedCertificate)
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

            return this.View(new ImportQuestionnaireListModel
            {
                DesignerUserName = this.designerUserCredentials.Get().Login,
                LogoutFromDesigner = Url.Action("LogoutFromDesigner"),
                SurveySetup = Url.Action("Index", "SurveySetup"),
                Import = Url.Action("Import"),
                DataUrl = Url.Action("QuestionnairesList", "DesignerQuestionnairesApi"),
                ImportMode = Url.Action("ImportMode")
            });
        }
      
        public async Task<ActionResult> ImportMode(Guid id)
        {
            if (this.designerUserCredentials.Get() == null)
            {
                //Error(Resources.LoginToDesigner.SessionExpired);
                return this.RedirectToAction("LoginToDesigner");
            }

            var model = await this.GetImportModel(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Import(Guid id, ImportModel request)
        {
            if (this.designerUserCredentials.Get() == null)
            {
                //Error(Resources.LoginToDesigner.SessionExpired);
                return this.RedirectToAction("LoginToDesigner");
            }

            var model = await this.GetImportModel(id);
            if (model.QuestionnaireInfo != null)
            {
                var result = await this.importService.Import(id, model.QuestionnaireInfo?.Name, false,
                    request.Comment, Request.GetDisplayUrl());
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
                        this.upgradeService.EnqueueUpgrade(processId, authorizedUser.Id, sourceQuestionnaireId, result.Identity);
                        return RedirectToAction("UpgradeProgress", "SurveySetup", new {id = processId});
                    }

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
                var questionnaireInfo = await this.designerApi.GetQuestionnaireInfo(id);

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


        [AntiForgeryFilter]
        public IActionResult LoginToDesigner()
        {
            return this.View(new
            {
                BackLink = Url.Action("Index", "SurveySetup"),
                LoginAction = Url.Action("LoginToDesigner", "Template"),
                DesignerLogo = Url.Content("~/img/designer-logo.png"),
                ListUrl = Url.Action("Import")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginToDesigner([FromBody]LogOnModel model)
        {
            var creds = new RestCredentials { Login = model.UserName, Password = model.Password };

            try
            {
                var authHeader = creds.GetAuthenticationHeaderValue();
                await this.designerApi.Login(authHeader.ToString());
                
                this.designerUserCredentials.Set(creds);

                return Ok();
            }
            catch (RestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
            catch (RestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                var position = ex.Message.IndexOf("IP:", StringComparison.InvariantCultureIgnoreCase);
                string ipString = position > -1 ? ex.Message.Substring(position) : "";


                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    Message = $"{Resources.LoginToDesigner.AccessForbidden} {ipString}"
                });
            }
            catch (RestException ex)
            {
                this.logger.LogWarning(ex, "Error communicating to designer");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Could not connect to designer.");
            }

            return StatusCode(StatusCodes.Status400BadRequest, new
            {
                Message = string.Format(
                    QuestionnaireImport.LoginToDesignerError,
                    Url.Action("Import", "Template"))
            });
        }
    }

    public class ImportModel
    {
        public bool ShouldMigrateAssignments { get; set; }

        public string MigrateFrom { get; set; }
        public string Comment { get; set; }
    }
}
