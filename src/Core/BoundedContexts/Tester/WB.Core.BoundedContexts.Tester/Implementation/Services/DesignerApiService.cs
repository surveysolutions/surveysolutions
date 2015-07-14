using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    internal class DesignerApiService : IDesignerApiService
    {
        private readonly ILogger logger;
        private readonly IRestService restService;
        private readonly IUserIdentity userIdentity;

        public DesignerApiService(ILogger logger, 
            IRestService restService, 
            IUserIdentity userIdentity)
        {
            this.logger = logger;
            this.restService = restService;
            this.userIdentity = userIdentity;
        }

        public async Task<bool> Authorize(string login, string password)
        {
            await this.restService.GetAsync(
                url: "login",
                credentials: new RestCredentials()
                {
                    Login = login,
                    Password = password
                });

            return true;
        }

        public async virtual Task<IList<QuestionnaireListItem>> GetQuestionnairesAsync(bool isPublic, CancellationToken token)
        {
            var pageIndex = 1;
            var serverQuestionnaires = new List<QuestionnaireListItem>();

            QuestionnaireListItem[] batchOfServerQuestionnaires;
            do
            {
                batchOfServerQuestionnaires = await this.GetPageOfQuestionnairesAsync(isPublic: isPublic, pageIndex: pageIndex++, token: token);
                serverQuestionnaires.AddRange(batchOfServerQuestionnaires);

            } while (batchOfServerQuestionnaires.Any());

            return serverQuestionnaires;
        }

        public async Task<Questionnaire> GetQuestionnaireAsync(QuestionnaireListItem selectedQuestionnaire, Action<decimal> downloadProgress, CancellationToken token)
        {
            Questionnaire downloadedQuestionnaire = null;

            downloadedQuestionnaire = await this.restService.GetWithProgressAsync<Questionnaire>(
                url: string.Format("questionnaires/{0}", selectedQuestionnaire.Id),
                credentials:
                    new RestCredentials()
                    {
                        Login = this.userIdentity.Name,
                        Password = this.userIdentity.Password
                    },
                progressPercentage: downloadProgress, token: token);

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
    }
}