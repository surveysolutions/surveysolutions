using System;
using System.Net;
using System.ServiceModel.Security;
using System.Web.Mvc;
using Main.Core.View;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Views;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.Template;
using WB.UI.Headquarters.Utils;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class QuestionnairesController : BaseController
    {
        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory;
        private readonly IDesignerService designerService;
        private readonly ILogger logger;

        public QuestionnairesController(IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory, 
            IDesignerService designerService, 
            SslSettings sslSettings,
            ILogger logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.designerService = designerService;
            this.logger = logger;
            if (sslSettings.AcceptUnsignedCertificate)
            {
                ServicePointManager.ServerCertificateValidationCallback = (self, certificate, chain, sslPolicyErrors) => true;
            }
        }

        public ActionResult Index()
        {
            QuestionnaireBrowseView model = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel {
                PageSize = 1024
            });
            return this.View(model);
        }

        public ActionResult Import(QuestionnaireListInputModel model)
        {
            if (!this.designerService.CurrentUserIsLoggedIn())
            {
                return this.RedirectToAction("LoginToDesigner");
            }

            return
                this.View();
        }

        public ActionResult LoginToDesigner()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult LoginToDesigner(LogOnModel model)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    this.designerService.TryLogin(model.UserName, model.Password);

                    return this.RedirectToAction("Import");
                }
                catch (MessageSecurityException)
                {
                    this.Error("Incorrect UserName/Password");
                }
                catch (Exception ex)
                {
                    this.Error(
                        string.Format("Could not connect to designer. Please check that designer is available and try <a href='{0}'>again</a>",
                            GlobalHelper.GenerateUrl("Import", "Questionnaires", null)));
                    this.logger.Error("Could not connect to designer.", ex);
                }
            }

            return this.View(model);
        }
    }
}