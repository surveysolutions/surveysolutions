using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Template;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class TemplateController : BaseController
    {
        private readonly IRestService designerQuestionnaireApiRestService;

        public TemplateController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, IRestService designerQuestionnaireApiRestService)
            : base(commandService, globalInfo, logger)
        {
            this.designerQuestionnaireApiRestService = designerQuestionnaireApiRestService;
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

        public ActionResult LoginToDesigner()
        {
            this.Attention(QuestionnaireImport.BeforeSignInToDesigner);
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginToDesigner(LogOnModel model)
        {
            if (this.ModelState.IsValid)
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
                    this.Error(ex.Message);
                }
                catch (Exception ex)
                {
                    this.Logger.Error("Could not connect to designer.", ex);

                    this.Error(string.Format(
                            QuestionnaireImport.LoginToDesignerError,
                            GlobalHelper.GenerateUrl("Import", "Template", new { area = string.Empty })));
                    
                }
            }

            return this.View(model);
        }
    }
}