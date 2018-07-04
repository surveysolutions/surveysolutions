using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Tester.Infrastructure.Internals.Security;
using QuestionnaireListItem = WB.Core.BoundedContexts.Tester.Views.QuestionnaireListItem;

namespace WB.UI.Tester.Infrastructure.Internals
{
    internal class DesignerApiService : IDesignerApiService
    {
        private readonly string apiPrefix = $"/api/v{ApiVersion.CurrentTesterProtocolVersion}";

        private readonly IRestService restService;
        private readonly IPrincipal principal;

        private RestCredentials RestCredentials => new RestCredentials
        {
            Login = this.principal.CurrentUserIdentity.Name,
            Password = ((TesterUserIdentity)this.principal.CurrentUserIdentity).Password
        };

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
                url: $"{this.apiPrefix}/user/login",
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

        public async Task<Questionnaire> GetQuestionnaireAsync(string questionnaireId, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, CancellationToken token)
        {
            Questionnaire downloadedQuestionnaire = null;

            downloadedQuestionnaire = await this.restService.GetAsync<Questionnaire>(
                url: $"{this.apiPrefix}/questionnaires/{questionnaireId}",
                credentials: this.RestCredentials,
                onDownloadProgressChanged: onDownloadProgressChanged, token: token);

            return downloadedQuestionnaire;
        }

        public async Task<AttachmentContent> GetAttachmentContentAsync(string attachmentContentId,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, 
            CancellationToken token)
        {
            var restFile = await this.restService.DownloadFileAsync(
                url: $"{this.apiPrefix}/attachment/{attachmentContentId}",
                credentials: this.RestCredentials,
                onDownloadProgressChanged: onDownloadProgressChanged,
                token: token).ConfigureAwait(false);

            var attachmentContent = new AttachmentContent()
            {
                ContentType = restFile.ContentType,
                Content = restFile.Content,
                Id = restFile.ContentHash.Trim('"'),
                Size = restFile.ContentLength ?? restFile.Content.LongLength
            };

            return attachmentContent;
        }

        public Task<TranslationDto[]> GetTranslationsAsync(string questionnaireId, CancellationToken token)
            => this.restService.GetAsync<TranslationDto[]>(
                url: $"{this.apiPrefix}/translation/{questionnaireId}",
                token: token,
                credentials: this.RestCredentials);

        private async Task<QuestionnaireListItem[]> GetPageOfQuestionnairesAsync(int pageIndex, CancellationToken token)
        {
            var  batchOfServerQuestionnaires = await this.restService.GetAsync<Core.SharedKernels.SurveySolutions.Api.Designer.TesterQuestionnaireListItem[]>(
                url: $"{this.apiPrefix}/questionnaires",
                token: token,
                credentials: this.RestCredentials,
                queryString: new { pageIndex = pageIndex });

            return batchOfServerQuestionnaires.Select(questionnaireListItem => new QuestionnaireListItem()
            {
                Id = questionnaireListItem.Id,
                Title = questionnaireListItem.Title,
                LastEntryDate = questionnaireListItem.LastEntryDate,
                IsPublic = questionnaireListItem.IsPublic,
                OwnerName = questionnaireListItem.Owner,
                IsOwner = questionnaireListItem.IsOwner,
                IsShared = questionnaireListItem.IsShared
            }).ToArray();
        }
    }
}
