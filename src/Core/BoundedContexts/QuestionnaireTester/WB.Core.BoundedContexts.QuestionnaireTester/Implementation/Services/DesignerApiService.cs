using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using QuestionnaireListItem = WB.Core.BoundedContexts.QuestionnaireTester.Views.QuestionnaireListItem;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    public class DesignerApiService
    {
        private readonly ILogger logger;
        private readonly IRestService restService;
        private readonly IUserIdentity userIdentity;
        private readonly IUserInteraction userInteraction;

        public DesignerApiService(ILogger logger, 
            IRestService restService, 
            ISettingsProvider settingsProvider, 
            IUserIdentity userIdentity, 
            IUserInteraction userInteraction)
        {
            this.logger = logger;
            this.restService = restService;
            this.userIdentity = userIdentity;
            this.userInteraction = userInteraction;
        }

        public async Task<bool> Authorize(string login, string password)
        {
            bool isUserAuthrizedOnServer = false;
            try
            {
                await this.restService.GetAsync(
                    url: "login",
                    credentials: new RestCredentials()
                    {
                        Login = login,
                        Password = password
                    });
                isUserAuthrizedOnServer = true;
            }
            catch (RestException ex)
            {
                string errorMessage = this.GetErrorMessageByGeneralHttpStatuses(ex);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    switch (ex.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            errorMessage = UIResources.Login_Error_NotFound;
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    this.userInteraction.Alert(errorMessage);
                else throw;
            }

            return isUserAuthrizedOnServer;
        }

        public async Task GetQuestionnairesAsync(bool isPublic, Action<QuestionnaireListItem[]> onPageReceived, CancellationToken token)
        {
            var pageIndex = 1;

            try
            {
                QuestionnaireListItem[] batchOfServerQuestionnaires;
                do
                {
                    batchOfServerQuestionnaires = await this.GetPageOfQuestionnairesAsync(isPublic: isPublic, pageIndex: pageIndex++, token: token);
                    if (onPageReceived != null)
                    {
                        onPageReceived(batchOfServerQuestionnaires);
                    }

                } while (batchOfServerQuestionnaires.Any());
            }
            catch (RestException ex)
            {
                string errorMessage = this.GetErrorMessageByGeneralHttpStatuses(ex);

                if (!string.IsNullOrEmpty(errorMessage))
                    this.userInteraction.Alert(errorMessage);
                else throw;
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
                            Login = this.userIdentity.Name,
                            Password = this.userIdentity.Password
                        },
                    progressPercentage: downloadProgress, token: token);
            }
            catch (RestException ex)
            {
                string errorMessage = this.GetErrorMessageByGeneralHttpStatuses(ex);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    switch (ex.StatusCode)
                    {
                        case HttpStatusCode.Forbidden:
                            errorMessage = UIResources.ImportQuestionnaire_Error_Forbidden;
                            break;
                        case HttpStatusCode.PreconditionFailed:
                            errorMessage = UIResources.ImportQuestionnaire_Error_PreconditionFailed;
                            break;
                        case HttpStatusCode.NotFound:
                            errorMessage = UIResources.ImportQuestionnaire_Error_NotFound;
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    this.userInteraction.Alert(errorMessage);
                else throw;
            }
            catch (OperationCanceledException)
            {
                // show here the message that loading questionnaire was canceled
                // don't needed in the current implementation
            }

            return downloadedQuestionnaire;
        }

        private async Task<QuestionnaireListItem[]> GetPageOfQuestionnairesAsync(bool isPublic, int pageIndex, CancellationToken token)
        {
            var  batchOfServerQuestionnaires= await this.restService.GetAsync<SharedKernels.SurveySolutions.Api.Designer.QuestionnaireListItem[]>(
                url: "questionnaires",
                token: token,
                credentials:
                    new RestCredentials()
                    {
                        Login = this.userIdentity.Name,
                        Password = this.userIdentity.Password
                    },
                queryString: new { pageIndex = pageIndex, isPublic = isPublic });

            return batchOfServerQuestionnaires.Select(questionnaireListItem => new QuestionnaireListItem()
            {
                Id = questionnaireListItem.Id,
                Title = questionnaireListItem.Title,
                LastEntryDate = questionnaireListItem.LastEntryDate,
                IsPublic = isPublic,
                OwnerName = this.userIdentity.Name
            }).ToArray();
        }

        private string GetErrorMessageByGeneralHttpStatuses(RestException ex)
        {
            string errorMessage = string.Empty;

            switch (ex.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    errorMessage = ex.Message.Contains("lock") ? UIResources.AccountIsLockedOnServer : UIResources.Unauthorized;
                    break;
                case HttpStatusCode.ServiceUnavailable:
                    errorMessage = ex.Message.Contains("maintenance") ? UIResources.Maintenance : UIResources.ServiceUnavailable;
                    break;
                case HttpStatusCode.RequestTimeout:
                    errorMessage = UIResources.RequestTimeout;
                    break;
                case HttpStatusCode.UpgradeRequired:
                    errorMessage = UIResources.ImportQuestionnaire_Error_UpgradeRequired;
                    break;
                case HttpStatusCode.InternalServerError:
                    this.logger.Error("Internal server error when login.", ex);
                    errorMessage = UIResources.InternalServerError;
                    break;
            }

            return errorMessage;
        }
    }
}