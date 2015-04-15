using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.QuestionnaireTester.Authentication;

namespace WB.UI.QuestionnaireTester.Services
{
    public class DesignerService
    {
        private readonly IRestService restService;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;

        public DesignerService(IRestService restService, IQuestionnaireVersionProvider questionnaireVersionProvider)
        {
            this.restService = restService;
            this.questionnaireVersionProvider = questionnaireVersionProvider;
        }

        public async Task<bool> Login(string userName, string password, CancellationToken cancellationToken)
        {
            try
            {
                await restService.GetAsync(url: "login", credentials: new RestCredentials() {Login = userName, Password = password});
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<QuestionnaireListCommunicationPackage> GetQuestionnaireListForCurrentUser(UserInfo remoteUser, CancellationToken cancellationToken)
        {
            return await restService.GetAsync<QuestionnaireListCommunicationPackage>(
                url: "questionnairelist",
                credentials: new RestCredentials() {Login = remoteUser.UserName, Password = remoteUser.Password});

        }

        public Task<QuestionnaireCommunicationPackage> GetTemplateForCurrentUser(UserInfo remoteUser, Guid id, CancellationToken cancellationToken)
        {
            var supportedVersion = questionnaireVersionProvider.GetCurrentEngineVersion();

            return this.restService.PostAsync<QuestionnaireCommunicationPackage>(
                url: "questionnaire",
                credentials: new RestCredentials() {Login = remoteUser.UserName, Password = remoteUser.Password},
                request: new DownloadQuestionnaireRequest()
                {
                    QuestionnaireId = id,
                    SupportedVersion = new QuestionnnaireVersion()
                    {
                        Major = supportedVersion.Major,
                        Minor = supportedVersion.Minor,
                        Patch = supportedVersion.Patch
                    }
                });
        }
    }
}