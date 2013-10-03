using System;
using System.ServiceModel.Security;
using System.Web.Mvc;
using Core.Supervisor.Views.Template;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using Web.Supervisor.DesignerPublicService;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter")]
    public class TemplateController : BaseController
    {
        public TemplateController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
            : base(commandService, globalInfo, logger)
        {
            this.ViewBag.ActivePage = MenuItem.Administration;
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
                            GlobalHelper.GenerateUrl("Import", "Template", null)));
                    this.Logger.Error("Could not connect to designer.", ex);
                }
            }

            return this.View(model);
        }
    }
}