using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernel.Utils.Implementation.Services;
using WB.Core.SharedKernel.Utils.Services.Rest;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.QuestionnaireTester.Authentication;

namespace WB.UI.QuestionnaireTester.Services
{
    public class DesignerService
    {
        private readonly IRestService restService;

        public DesignerService(IRestService restService)
        {
            this.restService = restService;
        }

        public async Task<bool> Login(string userName, string password, CancellationToken cancellationToken)
        {
            try
            {
                await restService.GetAsync(url: "validatecredentials", token: cancellationToken, credentials: new RestCredentials() {Login = userName, Password = password});
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
                token: cancellationToken,
                credentials: new RestCredentials() {Login = remoteUser.UserName, Password = remoteUser.Password});

        }

        public Task<QuestionnaireCommunicationPackage> GetTemplateForCurrentUser(UserInfo remoteUser, Guid id, CancellationToken cancellationToken)
        {
            var supportedVersion = QuestionnaireVersionProvider.GetCurrentEngineVersion();

            return this.restService.PostAsync<QuestionnaireCommunicationPackage>(
                url: "questionnaire",
                credentials: new RestCredentials() {Login = remoteUser.UserName, Password = remoteUser.Password},
                request: new DownloadQuestionnaireRequest()
                {
                    QuestionnaireId = id,
                    SupportedVersionMajor = supportedVersion.Major,
                    SupportedVersionMinor = supportedVersion.Minor,
                    SupportedVersionPatch = supportedVersion.Patch

                }, token: cancellationToken);
        }
    }
}