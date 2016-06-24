using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Template;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ApiValidationAntiForgeryToken]
    public class DesignerQuestionnairesApiController : BaseApiController
    {
        private readonly string apiPrefix = @"/api/hq";
        private readonly string apiVersion = @"v3";

        private readonly IAttachmentContentService attachmentContentService;

        internal RestCredentials designerUserCredentials
        {
            get { return this.getDesignerUserCredentials(this.GlobalInfo); }
            set { SetDesignerUserCredentials(this.GlobalInfo, value); }
        }

        private readonly IRestService restService;
        private readonly IStringCompressor zipUtils;
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly Func<IGlobalInfoProvider, RestCredentials> getDesignerUserCredentials;

        public DesignerQuestionnairesApiController(
            ISupportedVersionProvider supportedVersionProvider,
            ICommandService commandService, IGlobalInfoProvider globalInfo, IStringCompressor zipUtils, ILogger logger, IRestService restService,
            IAttachmentContentService questionnaireAttachmentService)
            : this(supportedVersionProvider, commandService, globalInfo, zipUtils, logger, GetDesignerUserCredentials, restService, questionnaireAttachmentService)
        {
            
        }

        internal DesignerQuestionnairesApiController(ISupportedVersionProvider supportedVersionProvider,
            ICommandService commandService, IGlobalInfoProvider globalInfo, IStringCompressor zipUtils, ILogger logger,
            Func<IGlobalInfoProvider, RestCredentials> getDesignerUserCredentials, IRestService restService,
            IAttachmentContentService attachmentContentService)
            : base(commandService, globalInfo, logger)
        {
            this.zipUtils = zipUtils;
            this.getDesignerUserCredentials = getDesignerUserCredentials;
            this.supportedVersionProvider = supportedVersionProvider;
            this.restService = restService;
            this.attachmentContentService = attachmentContentService;
        }

        private static RestCredentials GetDesignerUserCredentials(IGlobalInfoProvider globalInfoProvider)
        {
            return (RestCredentials)HttpContext.Current.Session[globalInfoProvider.GetCurrentUser().Name];
        }

        private static void SetDesignerUserCredentials(IGlobalInfoProvider globalInfoProvider, RestCredentials designerUserCredentials)
        {
            HttpContext.Current.Session[globalInfoProvider.GetCurrentUser().Name] = designerUserCredentials;
        }

        public async Task<DesignerQuestionnairesView> QuestionnairesList(DesignerQuestionnairesListViewModel data)
        {
            var list = await this.restService.GetAsync<PagedQuestionnaireCommunicationPackage>(
                url: $"{this.apiPrefix}/{this.apiVersion}/questionnaires",
                credentials: this.designerUserCredentials,
                queryString: new
                {
                    Filter = data.Filter,
                    PageIndex = data.PageIndex,
                    PageSize = data.PageSize,
                    SortOrder = data.SortOrder.GetOrderRequestString()
                });

            return new DesignerQuestionnairesView()
                {
                    Items = list.Items.Select(x => new DesignerQuestionnaireListViewItem() { Id = x.Id, Title = x.Title }),
                    TotalCount = list.TotalCount
                };
        }

        [HttpPost]
        public async Task<QuestionnaireVerificationResponse> GetQuestionnaire(ImportQuestionnaireRequest request)
        {
            try
            {
                var supportedVersion = this.supportedVersionProvider.GetSupportedQuestionnaireVersion();

                var questionnairePackage = await this.restService.GetAsync<QuestionnaireCommunicationPackage>(
                    url: $"{this.apiPrefix}/{this.apiVersion}/questionnaires/{request.Questionnaire.Id}",
                    credentials: designerUserCredentials,
                    queryString: new { clientQuestionnaireContentVersion = supportedVersion });

                var questionnaire = this.zipUtils.DecompressString<QuestionnaireDocument>(questionnairePackage.Questionnaire);
                var questionnaireContentVersion = questionnairePackage.QuestionnaireContentVersion;
                var questionnaireAssembly = questionnairePackage.QuestionnaireAssembly;

                if (questionnaire.Attachments != null)
                {
                    foreach (var questionnaireAttachment in questionnaire.Attachments)
                    {
                        if(this.attachmentContentService.HasAttachmentContent(questionnaireAttachment.ContentId))
                            continue;

                        var attachmentContent = await this.restService.DownloadFileAsync(
                            url: $"{this.apiPrefix}/attachment/{questionnaireAttachment.ContentId}",
                            credentials: designerUserCredentials);

                        this.attachmentContentService.SaveAttachmentContent(questionnaireAttachment.ContentId,
                            attachmentContent.ContentType, attachmentContent.Content);
                    }
                }
                
                this.CommandService.Execute(new ImportFromDesigner(
                    this.GlobalInfo.GetCurrentUser().Id, 
                    questionnaire,
                    request.AllowCensusMode, 
                    questionnaireAssembly,
                    questionnaireContentVersion));

                return new QuestionnaireVerificationResponse();
            }
            catch (RestException ex)
            {
                var questionnaireVerificationResponse = new QuestionnaireVerificationResponse();

                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.UpgradeRequired:
                    case HttpStatusCode.ExpectationFailed:
                        questionnaireVerificationResponse.ImportError = ex.Message;
                        break;
                    case HttpStatusCode.PreconditionFailed:
                        questionnaireVerificationResponse.ImportError = string.Format(ErrorMessages.Questionnaire_verification_failed, request.Questionnaire.Title);
                        break;
                    case HttpStatusCode.NotFound:
                        questionnaireVerificationResponse.ImportError = string.Format(ErrorMessages.TemplateNotFound, request.Questionnaire.Title);
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        questionnaireVerificationResponse.ImportError = ErrorMessages.ServiceUnavailable;
                        break;
                    case HttpStatusCode.RequestTimeout:
                        questionnaireVerificationResponse.ImportError = ErrorMessages.RequestTimeout;
                        break;
                    default:
                        questionnaireVerificationResponse.ImportError = ErrorMessages.ServerError;
                        break;
                }

                return questionnaireVerificationResponse;
            }
            catch (Exception ex)
            {
                var domainEx = ex.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx != null) return new QuestionnaireVerificationResponse() {ImportError = domainEx.Message};

                this.Logger.Error(
                    string.Format("Designer: error when importing template #{0}", request.Questionnaire.Id), ex);
                throw;
            }
        }
    }
}