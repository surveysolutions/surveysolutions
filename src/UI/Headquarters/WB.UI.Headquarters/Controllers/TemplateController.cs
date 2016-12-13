using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Template;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Models;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class TemplateController : BaseController
    {
        private readonly IRestService designerQuestionnaireApiRestService;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;

        public TemplateController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, IRestService designerQuestionnaireApiRestService, IQuestionnaireVersionProvider questionnaireVersionProvider, IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.designerQuestionnaireApiRestService = designerQuestionnaireApiRestService;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
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

        public ActionResult Import(QuestionnaireListInputModel model)
        {
            if (this.designerUserCredentials == null)
            {
                return this.RedirectToAction("LoginToDesigner");
            }

            return
                this.View("ImportOld");
        }

        public async Task<ActionResult> ImportMode(Guid id)
        {
            //if (this.designerUserCredentials == null)
            //{
            //    return this.RedirectToAction("LoginToDesigner");
            //}

            var questionnaireInfo = await this.designerQuestionnaireApiRestService
                                              .GetAsync<QuestionnaireInfo>(url: $"/api/hq/v3/questionnaires/info/{id}", 
                                                                           credentials: new RestCredentials
                                                                           {
                                                                               Login = "Admin",
                                                                               Password = "q"
                                                                           });
            var model = new ImportModeModel();
            model.QuestionnaireInfo = questionnaireInfo;
            model.NewVersionNumber = this.questionnaireVersionProvider.GetNextVersion(id);
            return View(model);
        }

        public ActionResult LoginToDesigner()
        {
            this.Attention(QuestionnaireImport.BeforeSignInToDesigner);
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginToDesigner(LogOnModel model)
        {
            var designerUserCredentials = new RestCredentials {Login = model.UserName, Password = model.Password};

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