using System;
using System.Net;
using System.ServiceModel.Security;
using System.Web.Mvc;
using Core.Supervisor.Views.Template;
using Main.Core.Documents;
using Main.Core.Utility;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using Web.Supervisor.DesignerPublicService;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    /// <summary>
    ///     The template controller.
    /// </summary>
    [Authorize(Roles = "Headquarter")]
    public class TemplateController : BaseController
    {
        private readonly IStringCompressor zipUtils;

        public TemplateController(ICommandService commandService, IGlobalInfoProvider globalInfo, IStringCompressor zipUtils, ILogger logger)
            : base(commandService, globalInfo, logger)
        {
            this.zipUtils = zipUtils;

            this.ViewBag.ActivePage = MenuItem.Administration;

            #warning Roma: need to be deleted when we getting valid ssl certificate for new designer
            ServicePointManager.ServerCertificateValidationCallback =
                (self, certificate, chain, sslPolicyErrors) => true;
        }

        private IPublicService DesignerService
        {
            get { return this.DesignerServiceClient; }
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
                this.View(
                    this.DesignerService.GetQuestionnaireList(
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


        public ActionResult List(GridDataRequestModel data)
        {
            QuestionnaireListViewMessage list =
                this.DesignerService.GetQuestionnaireList(
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
                RemoteFileInfo docSource = this.DesignerService.DownloadQuestionnaire(new DownloadQuestionnaireRequest(id));
                document = this.zipUtils.Decompress<QuestionnaireDocument>(docSource.FileByteStream);
            }
            catch (Exception ex)
            {
                this.Error("Error when downloading questionnaire from designer. Please try again");
                this.Logger.Error("Unexpected error occurred", ex);
            }

            if (document == null)
            {
                return this.RedirectToAction("Import");
            }
            else
            {
                this.CommandService.Execute(new ImportQuestionnaireCommand(this.GlobalInfo.GetCurrentUser().Id, document));

                return this.RedirectToAction("Index", "HQ");
            }
        }
    }
}