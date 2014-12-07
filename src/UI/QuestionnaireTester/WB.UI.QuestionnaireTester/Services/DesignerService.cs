using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Implementation.Services.Rest;
using WB.Core.GenericSubdomains.Utils.Services.Rest;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
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
                await
                    restService.PostAsync(url: "ValidateCredentials", token: cancellationToken,
                        credentials: new RestCredentials() {Login = userName, Password = password});
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<QuestionnaireListCommunicationPackage> GetQuestionnaireListForCurrentUser(UserInfo remoteUser, CancellationToken cancellationToken)
        {
            return
                await
                    restService.GetAsync<QuestionnaireListCommunicationPackage>(url: "GetAllTemplates",
                        token: cancellationToken,
                        credentials: new RestCredentials() {Login = remoteUser.UserName, Password = remoteUser.Password});

        }

        public async Task<QuestionnaireCommunicationPackage> GetTemplateForCurrentUser(UserInfo remoteUser, Guid id, CancellationToken cancellationToken)
        {
            return
                await
                    restService.PostAsync<QuestionnaireCommunicationPackage>(url: "GetTemplate", token: cancellationToken,
                        credentials: new RestCredentials() {Login = remoteUser.UserName, Password = remoteUser.Password},
                        requestData:
                            new {id = id, maxSupportedVersion = QuestionnaireVersionProvider.GetCurrentEngineVersion()});

        }
    }
}