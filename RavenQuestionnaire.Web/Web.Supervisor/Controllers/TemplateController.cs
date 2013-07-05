using Main.Core.Commands.Questionnaire;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Net;
    using System.ServiceModel.Security;
    using System.Web.Mvc;

    using Core.Supervisor.Views.Template;

    using Main.Core.Documents;
    using Main.Core.Utility;
    
    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using WB.Core.SharedKernel.Utils.Compression;

    using Web.Supervisor.DesignerPublicService;
    using Web.Supervisor.Models;

    /// <summary>
    /// The template controller.
    /// </summary>
    [Authorize(Roles = "Headquarter")]
    public class TemplateController : BaseController
    {
        private readonly IStringCompressor zipUtils;
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
        /// <param name="logger">
        /// The logger.
        /// </param>
        public TemplateController(ICommandService commandService, IGlobalInfoProvider globalInfo, IStringCompressor zipUtils, ILogger logger)
            : base(commandService, globalInfo, logger)
        {
            this.zipUtils = zipUtils;

            ViewBag.ActivePage = MenuItem.Administration;

            #warning Roma: need to be deleted when we getting valid ssl certificate for new designer
            ServicePointManager.ServerCertificateValidationCallback =
                    (self, certificate, chain, sslPolicyErrors) => true;
        }

        #endregion

        private IPublicService DesignerService
        {
            get
            {
                return DesignerServiceClient;
            }
        }

        private PublicServiceClient DesignerServiceClient
        {
            get
            {
                return (PublicServiceClient)this.Session[GlobalInfo.GetCurrentUser().Name];
            }

            set
            {
                this.Session[GlobalInfo.GetCurrentUser().Name] = value;
            }
        }

        #region Public Methods and Operators

        /// <summary>
        /// The import.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Import(QuestionnaireListInputModel model)
        {
            if (this.DesignerServiceClient == null)
            {
                return this.RedirectToAction("LoginToDesigner");
            }

            return
                this.View(
                    DesignerService.GetQuestionnaireList(
                        new QuestionnaireListRequest(
                            Filter: string.Empty,
                            PageIndex: model.Page,
                            PageSize: model.PageSize,
                            SortOrder: model.Order)));
        }

        public ActionResult LoginToDesigner()
        {
            this.Attention("Before log in, please make sure that the 'Designer' website is available and it's not in the maintenance mode");
            return this.View();
        }

        [HttpPost]
        public ActionResult LoginToDesigner(LogOnModel model)
        {
            if (ModelState.IsValid)
            {
                var service = new PublicServiceClient();
                service.ClientCredentials.UserName.UserName = model.UserName;
                service.ClientCredentials.UserName.Password = model.Password;

                try
                {
                    service.Dummy();

                    DesignerServiceClient = service;

                    return RedirectToAction("Import");
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
                    Logger.Error(ex);
                }
            }

            return this.View(model);
        }


        /// <summary>
        /// Gets table data for some view
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// Partial view with table's body
        /// </returns>
        public ActionResult List(GridDataRequestModel data)
        {
            var list =
                DesignerService.GetQuestionnaireList(
                    new QuestionnaireListRequest(
                        Filter: string.Empty,
                        PageIndex: data.Pager.Page,
                        PageSize: data.Pager.PageSize,
                        SortOrder: StringUtil.GetOrderRequestString(data.SortOrder)));

            return this.PartialView("_PartialGrid_Questionnaires", list);
        }

        public ActionResult Get(Guid id)
        {
            QuestionnaireDocument document = null;

            try
            {
                var docSource = DesignerService.DownloadQuestionnaire(new DownloadQuestionnaireRequest(id));
                document = zipUtils.Decompress<QuestionnaireDocument>(docSource.FileByteStream);
            }
            catch (Exception ex)
            {
                this.Error("Error when downloading questionnaire from designer. Please try again");
                Logger.Error(ex);
            }

            if (document == null)
            {
                return this.RedirectToAction("Import");
            }
            else
            {
                this.CommandService.Execute(
                    new ImportQuestionnaireCommand(this.GlobalInfo.GetCurrentUser().Id, document));

                return this.RedirectToAction("Questionnaires", "Dashboard");    
            }
        }

        #endregion
    }
}