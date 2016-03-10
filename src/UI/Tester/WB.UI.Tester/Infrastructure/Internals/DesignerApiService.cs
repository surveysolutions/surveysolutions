using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using QuestionnaireListItem = WB.Core.BoundedContexts.Tester.Views.QuestionnaireListItem;

namespace WB.UI.Tester.Infrastructure.Internals
{
    internal class DesignerApiService : IDesignerApiService
    {
        private readonly IRestService restService;
        private readonly IPrincipal principal;

        public DesignerApiService(
            IRestService restService, 
            IPrincipal principal)
        {
            this.restService = restService;
            this.principal = principal;
        }

        public async Task<bool> Authorize(string login, string password)
        {
            await this.restService.GetAsync(
                url: "login",
                credentials: new RestCredentials()
                {
                    Login = login,
                    Password = password
                },
                forceNoCache: true);

            return true;
        }

        public async Task<IReadOnlyCollection<QuestionnaireListItem>> GetQuestionnairesAsync(CancellationToken token)
        {
            var pageIndex = 1;
            var serverQuestionnaires = new List<QuestionnaireListItem>();

            QuestionnaireListItem[] batchOfServerQuestionnaires;
            do
            {
                batchOfServerQuestionnaires = await this.GetPageOfQuestionnairesAsync(pageIndex: pageIndex++, token: token);
                serverQuestionnaires.AddRange(batchOfServerQuestionnaires);

            } while (batchOfServerQuestionnaires.Any());

            return serverQuestionnaires.ToReadOnlyCollection();
        }

        public async Task<Questionnaire> GetQuestionnaireAsync(QuestionnaireListItem selectedQuestionnaire, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, CancellationToken token)
        {
            Questionnaire downloadedQuestionnaire = null;

            downloadedQuestionnaire = await this.restService.GetAsync<Questionnaire>(
                url: $"questionnaires/{selectedQuestionnaire.Id}",
                credentials:
                    new RestCredentials
                    {
                        Login = this.principal.CurrentUserIdentity.Name,
                        Password = this.principal.CurrentUserIdentity.Password
                    },
                onDownloadProgressChanged: onDownloadProgressChanged, token: token);

            return downloadedQuestionnaire;
        }

        public async Task<byte[]> GetQuestionnaireAttachmentAsync(string attachmentId,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, 
            CancellationToken token)
        {
            var attachmentContent = await this.restService.DownloadFileAsync(
                url: $"attachment/{attachmentId}",
                credentials:
                    new RestCredentials
                    {
                        Login = this.principal.CurrentUserIdentity.Name,
                        Password = this.principal.CurrentUserIdentity.Password
                    },
                onDownloadProgressChanged: onDownloadProgressChanged,
                token: token);

            return attachmentContent;
        }

        public async Task<string[]> GetQuestionnaireAttachmentIdsAsync(QuestionnaireListItem selectedQuestionnaire, 
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
            CancellationToken token)
        {
            var attachmentsIds = await this.restService.GetAsync<string[]>(
                url: $"attachments/{selectedQuestionnaire.Id}",
                credentials:
                    new RestCredentials
                    {
                        Login = this.principal.CurrentUserIdentity.Name,
                        Password = this.principal.CurrentUserIdentity.Password
                    },
                onDownloadProgressChanged: onDownloadProgressChanged, 
                token: token);

            return attachmentsIds;
        }

        private async Task<QuestionnaireListItem[]> GetPageOfQuestionnairesAsync(int pageIndex, CancellationToken token)
        {
            var  batchOfServerQuestionnaires = await this.restService.GetAsync<Core.SharedKernels.SurveySolutions.Api.Designer.QuestionnaireListItem[]>(
                url: "questionnaires",
                token: token,
                credentials: 
                    new RestCredentials
                    {
                        Login = this.principal.CurrentUserIdentity.Name,
                        Password = this.principal.CurrentUserIdentity.Password
                    },
                queryString: new { pageIndex = pageIndex });

            return batchOfServerQuestionnaires.Select(questionnaireListItem => new QuestionnaireListItem()
            {
                Id = questionnaireListItem.Id,
                Title = questionnaireListItem.Title,
                LastEntryDate = questionnaireListItem.LastEntryDate,
                IsPublic = questionnaireListItem.IsPublic,
                OwnerName = questionnaireListItem.Owner,
                IsShared = questionnaireListItem.IsShared
            }).ToArray();
        }
    }
}