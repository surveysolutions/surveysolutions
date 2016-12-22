using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using FluentMigrator.Infrastructure;
using Flurl.Http;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.Models.Template;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class TemplateController : BaseController
    {
        private readonly IRestService designerQuestionnaireApiRestService;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private IQuestionnaireImportService importService;

        public TemplateController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, IRestService designerQuestionnaireApiRestService, IQuestionnaireVersionProvider questionnaireVersionProvider, IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory, IQuestionnaireImportService importService)
            : base(commandService, globalInfo, logger)
        {
            this.designerQuestionnaireApiRestService = designerQuestionnaireApiRestService;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
            this.importService = importService;
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (AppSettings.Instance.AcceptUnsignedCertificate)
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    (self, certificate, chain, sslPolicyErrors) => true;
            }
        }


        private RestCredentials designerUserCredentials
        {
            get { return (RestCredentials)this.Session[this.GlobalInfo.GetCurrentUser().Name]; }

            set { this.Session[this.GlobalInfo.GetCurrentUser().Name] = value; }
        }

        public ActionResult Import()
        {
            if (this.designerUserCredentials == null)
            {
                return this.RedirectToAction("LoginToDesigner");
            }

            return this.View(new ImportQuestionnaireListModel { DesignerUserName = this.designerUserCredentials.Login });
        }

      
        public async Task<ActionResult> ImportMode(Guid id, string questionnaireTitle)
        {
            if (this.designerUserCredentials == null)
            {
                return this.RedirectToAction("LoginToDesigner");
            }

            var model = await this.GetImportModel(id, questionnaireTitle);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ImportMode(Guid id, string name, string importMode)
        {
            if (this.designerUserCredentials == null)
            {
                return this.RedirectToAction("LoginToDesigner");
            }

            var result = await this.importService.Import(id, name, importMode == "Census");
            if (result.IsSuccess)
            {
                return this.RedirectToAction("Index", "HQ");
            }

            var model = await GetImportModel(id, name);
            model.ErrorMessage = result.ImportError;
            return this.View(model);
        }

        private async Task<ImportModeModel> GetImportModel(Guid id, string questionnaireTitle)
        {
            ImportModeModel model = new ImportModeModel();
            try
            {
                var questionnaireInfo = await this.designerQuestionnaireApiRestService
                    .GetAsync<QuestionnaireInfo>(url: $"/api/hq/v3/questionnaires/info/{id}",
                        credentials: this.designerUserCredentials);

                model.QuestionnaireInfo = questionnaireInfo;
                model.NewVersionNumber = this.questionnaireVersionProvider.GetNextVersion(id);
            }
            catch (RestException e)
            {
                switch (e.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        model.ErrorMessage = string.Format(ImportQuestionnaire.QuestionnaireCannotBeFound,
                            string.IsNullOrWhiteSpace(questionnaireTitle) ? string.Empty : $"\"{questionnaireTitle}\"");
                        break;
                    case HttpStatusCode.Forbidden:
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
            this.designerUserCredentials = null;
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
                await this.designerQuestionnaireApiRestService.GetAsync(url: @"/api/hq/user/login", credentials: designerUserCredentials);

                this.designerUserCredentials = designerUserCredentials;

                return this.RedirectToAction("Import");
            }
            catch (RestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    this.ModelState.AddModelError("InvalidCredentials", string.Empty);
                }
                else
                {
                    this.Logger.Warn("Error communicating to designer", ex);
                    this.Error(string.Format(
                        QuestionnaireImport.LoginToDesignerError,
                        GlobalHelper.GenerateUrl("Import", "Template", new { area = string.Empty })));
                }
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
}