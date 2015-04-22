using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using QuestionnaireListItem = WB.Core.BoundedContexts.QuestionnaireTester.Views.QuestionnaireListItem;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    public class DesignerApiService
    {
        private const int PageSize = 20;

        private readonly ILogger logger;
        private readonly IRestService restService;
        private readonly IRestServiceSettings restServiceSettings;
        private readonly IPrincipal principal;
        private readonly IUserInteraction userInteraction;

        public DesignerApiService(ILogger logger, IRestService restService, IRestServiceSettings restServiceSettings, IPrincipal principal, IUserInteraction userInteraction)
        {
            this.logger = logger;
            this.restService = restService;
            this.restServiceSettings = restServiceSettings;
            this.principal = principal;
            this.userInteraction = userInteraction;
        }

        public async Task Authorize(string login, string password)
        {
            try
            {
                await this.restService.GetAsync(
                    url: "login",
                    credentials: new RestCredentials()
                    {
                        Login = login,
                        Password = password
                    });
            }
            catch (RestException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        this.userInteraction.Alert(ex.Message.Contains("lock")
                            ? UIResources.AccountIsLockedOnServer
                            : UIResources.Unauthorized);
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        this.userInteraction.Alert(ex.Message.Contains("maintenance")
                            ? UIResources.Maintenance
                            : UIResources.ServiceUnavailable);
                        break;
                    case HttpStatusCode.RequestTimeout:
                        this.userInteraction.Alert(UIResources.RequestTimeout);
                        break;
                    case HttpStatusCode.InternalServerError:
                        this.logger.Error("Internal server error when login.", ex);
                        this.userInteraction.Alert(UIResources.InternalServerError);
                        break;
                    case HttpStatusCode.NotFound:
                        this.userInteraction.Alert(UIResources.Login_Error_NotFound);
                        break;
                    default:
                        throw;
                }
            }
        }

        public async Task GetQuestionnairesAsync(Action<QuestionnaireListItem[]> onPageReceived, CancellationToken token)
        {
            var pageIndex = 1;

            try
            {
                QuestionnaireListItem[] batchOfServerQuestionnaires;
                do
                {
                    batchOfServerQuestionnaires = await this.GetPageOfQuestionnairesAsync(pageIndex: pageIndex++, pageSize: PageSize, token: token);
                    if (onPageReceived != null)
                    {
                        onPageReceived(batchOfServerQuestionnaires);
                    }

                } while (batchOfServerQuestionnaires.Any());
            }
            catch (RestException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        this.userInteraction.Alert(ex.Message.Contains("lock")
                            ? UIResources.AccountIsLockedOnServer
                            : UIResources.Unauthorized);
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        this.userInteraction.Alert(ex.Message.Contains("maintenance")
                            ? UIResources.Maintenance
                            : UIResources.ServiceUnavailable);
                        break;
                    case HttpStatusCode.RequestTimeout:
                        this.userInteraction.Alert(UIResources.RequestTimeout);
                        break;
                    case HttpStatusCode.InternalServerError:
                        this.logger.Error("Internal server error when getting questionnaires.", ex);
                        this.userInteraction.Alert(UIResources.InternalServerError);
                        break;
                    default:
                        throw;
                }
            }
        }

        public async Task<Questionnaire> GetQuestionnaireAsync(QuestionnaireListItem selectedQuestionnaire, Action<decimal> downloadProgress, CancellationToken token)
        {
            Questionnaire downloadedQuestionnaire = null;
            try
            {

                downloadedQuestionnaire = await this.restService.GetWithProgressAsync<Questionnaire>(
                    url: string.Format("questionnaires/{0}", selectedQuestionnaire.Id),
                    credentials:
                        new RestCredentials()
                        {
                            Login = this.principal.CurrentUserIdentity.Name,
                            Password = this.principal.CurrentUserIdentity.Password
                        },
                    progressPercentage: downloadProgress, token: token);
            }
            catch (RestException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        this.userInteraction.Alert(ex.Message.Contains("lock")
                            ? UIResources.AccountIsLockedOnServer
                            : UIResources.Unauthorized);
                        break;
                    case HttpStatusCode.Forbidden:
                        this.userInteraction.Alert(string.Format(UIResources.ImportQuestionnaire_Error_Forbidden,
                            this.restServiceSettings.Endpoint.GetDomainName(), selectedQuestionnaire.Id,
                            selectedQuestionnaire.Title, selectedQuestionnaire.OwnerName));
                        break;
                    case HttpStatusCode.UpgradeRequired:
                        this.userInteraction.Alert(UIResources.ImportQuestionnaire_Error_UpgradeRequired);
                        break;
                    case HttpStatusCode.PreconditionFailed:
                        this.userInteraction.Alert(string.Format(UIResources.ImportQuestionnaire_Error_PreconditionFailed,
                            this.restServiceSettings.Endpoint.GetDomainName(), selectedQuestionnaire.Id, selectedQuestionnaire.Title));
                        break;
                    case HttpStatusCode.NotFound:
                        this.userInteraction.Alert(string.Format(UIResources.ImportQuestionnaire_Error_NotFound,
                            this.restServiceSettings.Endpoint.GetDomainName(), selectedQuestionnaire.Id, selectedQuestionnaire.Title));
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        this.userInteraction.Alert(ex.Message.Contains("maintenance")
                            ? UIResources.Maintenance
                            : UIResources.ServiceUnavailable);
                        break;
                    case HttpStatusCode.RequestTimeout:
                        this.userInteraction.Alert(UIResources.RequestTimeout);
                        break;
                    case HttpStatusCode.InternalServerError:
                        this.logger.Error("Internal server error when getting questionnaires.", ex);
                        this.userInteraction.Alert(UIResources.InternalServerError);
                        break;
                    default:
                        throw;
                }
            }
            catch (OperationCanceledException)
            {
                // show here the message that loading questionnaire was canceled
                // don't needed in the current implementation
            }

            return downloadedQuestionnaire;
        }

        private async Task<QuestionnaireListItem[]> GetPageOfQuestionnairesAsync(int pageIndex, int pageSize, CancellationToken token)
        {
            var  batchOfServerQuestionnaires= await this.restService.GetAsync<SharedKernels.SurveySolutions.Api.Designer.QuestionnaireListItem[]>(
                url: "questionnaires",
                token: token,
                credentials:
                    new RestCredentials()
                    {
                        Login = this.principal.CurrentUserIdentity.Name,
                        Password = this.principal.CurrentUserIdentity.Password
                    },
                queryString: new { pageIndex = pageIndex, pageSize = pageSize });

            return batchOfServerQuestionnaires.Select(questionnaireListItem => new QuestionnaireListItem()
            {
                Id = questionnaireListItem.Id,
                Title = questionnaireListItem.Title,
                LastEntryDate = questionnaireListItem.LastEntryDate,
                IsPublic = questionnaireListItem.IsPublic,
                OwnerName = this.principal.CurrentUserIdentity.Name
            }).ToArray();
        }
    }
}