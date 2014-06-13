using System;
using System.Net;
using System.ServiceModel.Security;
using System.Web.Mvc;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.SurveyManagement.Views.Template;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Supervisor.Code;
using WB.UI.Supervisor.DesignerPublicService;
using WB.UI.Supervisor.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Headquarter")]
    [Obsolete("Remove when HQ application will be separate")]
    public class TemplateController : BaseController
    {
        public TemplateController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
            : base(commandService, globalInfo, logger)
        {
            this.ViewBag.ActivePage = MenuItem.Administration;

            if (AppSettings.Instance.AcceptUnsignedCertificate)
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    (self, certificate, chain, sslPolicyErrors) => true;
            }
        }


        private PublicServiceClient DesignerServiceClient
        {
            get { return (PublicServiceClient) this.Session[this.GlobalInfo.GetCurrentUser().Name]; }

            set { this.Session[this.GlobalInfo.GetCurrentUser().Name] = value; }
        }

        public ActionResult Import(QuestionnaireListInputModel model)
        {
            if (this.DesignerServiceClient == null)
            {
                return this.RedirectToAction("LoginToDesigner");
            }

            return
                this.View();
        }

        public ActionResult LoginToDesigner()
        {
            this.Attention("Before log in, please make sure that the 'Designer' website is available and it's not in the maintenance mode");
            return this.View();
        }

        [HttpPost]
        public ActionResult LoginToDesigner(LogOnModel model)
        {
            if (this.ModelState.IsValid)
            {
                var service = new PublicServiceClient();
                service.ClientCredentials.UserName.UserName = model.UserName;
                service.ClientCredentials.UserName.Password = model.Password;

                try
                {
                    service.Dummy();

                    this.DesignerServiceClient = service;

                    return this.RedirectToAction("Import");
                }
                catch (MessageSecurityException)
                {
                    this.Error("Incorrect UserName/Password");
                }
                catch (Exception ex)
                {
                    this.Error(
                        string.Format(
                            "Could not connect to designer. Please check that designer is available and try <a href='{0}'>again</a>",
                            GlobalHelper.GenerateUrl("Import", "Template", new { area = string.Empty })));
                    this.Logger.Error("Could not connect to designer.", ex);
                }
            }

            return this.View(model);
        }
    }
}