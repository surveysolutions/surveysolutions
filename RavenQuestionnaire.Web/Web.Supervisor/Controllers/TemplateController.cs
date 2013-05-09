// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TemplateController.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.Web.Mvc;

    using Main.Core.Documents;
    using Main.Core.Utility;

    using NLog;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;

    using WB.Core.Questionnaire.ImportService.Commands;

    using Web.Supervisor.DesignerPublicService;
    using Web.Supervisor.Models;

    /// <summary>
    /// The template controller.
    /// </summary>
    [Authorize(Roles = "Headquarter")]
    public class TemplateController : BaseController
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateController"/> class.
        /// </summary>
        /// <param name="commandService">
        /// The command service.
        /// </param>
        /// <param name="globalInfo">
        /// The global info.
        /// </param>
        public TemplateController(ICommandService commandService, IGlobalInfoProvider globalInfo)
            : base(null, commandService, globalInfo)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The import.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Import()
        {
            ViewBag.ActivePage = MenuItem.Administration;
            return this.View(new ImportTemplateModel());
        }

        /// <summary>
        /// The import.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Import(ImportTemplateModel data)
        {
            ViewBag.ActivePage = MenuItem.Administration;
            if (this.ModelState.IsValid)
            {
                #warning Roma: need to be deleted when we getting valid ssl certificate for new designer
                ServicePointManager.ServerCertificateValidationCallback =
                        (self, certificate, chain, sslPolicyErrors) => true;

                try
                {
                    using (var service = new PublicServiceClient())
                    {
                        service.ClientCredentials.UserName.UserName = data.UserName;
                        service.ClientCredentials.UserName.Password = data.Password;

                        string document = service.DownloadQuestionnaireSource(data.QuestionnaireId);

                        this.CommandService.Execute(
                            new ImportQuestionnaireCommand(
                                this.GlobalInfo.GetCurrentUser().Id, document.DeserializeJson<QuestionnaireDocument>()));

                        return this.RedirectToAction("Questionnaires", "Dashboard");
                    }
                }
                catch (CommunicationObjectFaultedException)
                {
                    this.ViewBag.ErrorMessage = "You do not have permission to call this service";
                }
                catch (FaultException)
                {
                    this.ViewBag.ErrorMessage = "Check questionnaire id";
                }
                catch (Exception e)
                {
                    this.ViewBag.ErrorMessage = "Could not download template from designer. Please, try again later";
                    LogManager.GetCurrentClassLogger().Fatal("Error on import from designer ", e);
                }
            }

            return this.View(data);
        }

        #endregion
    }
}