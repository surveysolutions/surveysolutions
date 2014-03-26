using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Security;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.GenericSubdomains.Logging;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources.Questionnaires;

namespace WB.UI.Headquarters.Api
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class QuestionnairesController : ApiController
    {
        private readonly ILogger logger;
        private readonly IDesignerService designerService;

        public QuestionnairesController(ILogger logger, 
            IDesignerService designerService,
            SslSettings sslSettings)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            this.logger = logger;
            this.designerService = designerService;

            if (sslSettings.AcceptUnsignedCertificate)
            {
                ServicePointManager.ServerCertificateValidationCallback = (self, certificate, chain, sslPolicyErrors) => true;
            }
        }

        [HttpPost]
        public object LoginToDesigner(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, IndexPageResources.InvalidDesignerCredentials);
            }

            try
            {
                designerService.TryLogin(userName, password);

                return new
                {
                    success = true
                };
            }
            catch (MessageSecurityException ex)
            {
                var result = this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, IndexPageResources.InvalidDesignerCredentials, ex);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("Could not connect to designer.", ex);
                HttpResponseMessage errorResponse = this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, IndexPageResources.CouldNotConnectToDesigner, ex);
                return errorResponse;
            }
        }

        [HttpGet]
        public object List(DesignerQuestionnairesListModel data)
        {
            return new object();
            //QuestionnaireListViewMessage list =
            //    this.designerService.GetQuestionnaireList(
            //        new QuestionnaireListRequest(
            //            Filter: data.Filter,
            //            PageIndex: data.Page,
            //            PageSize: data.PageSize));

            //return new DesignerQuestionnairesView()
            //{
            //    Items = list.Items.Select(x => new DesignerQuestionnaireListViewItem() { Id = x.Id, Title = x.Title }),
            //    TotalCount = list.TotalCount,
            //    ItemsSummary = null
            //};
        }

    }
}