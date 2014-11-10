using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ninject;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.QuestionnaireTester.Authentication;

namespace WB.UI.QuestionnaireTester.Services
{
    public class DesignerService
    {
        private readonly IRestServiceWrapperFactory restUtilsFactory;

        public DesignerService()
        {
            restUtilsFactory = CapiTesterApplication.Kernel.Get<IRestServiceWrapperFactory>();
        }

        public async Task<bool> Login(string userName, string password, CancellationToken cancellationToken)
        {
            var webExecutor = restUtilsFactory.CreateRestServiceWrapper(CapiTesterApplication.GetPathToDesigner());
            try
            {
                return await webExecutor.ExecuteRestRequestAsync<bool>("ValidateCredentials", cancellationToken, null, userName, password, "POST");
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<QuestionnaireListCommunicationPackage> GetQuestionnaireListForCurrentUser(UserInfo remoteUser, CancellationToken cancellationToken)
        {
            var webExecutor = restUtilsFactory.CreateRestServiceWrapper(CapiTesterApplication.GetPathToDesigner());
            return await webExecutor.ExecuteRestRequestAsync<QuestionnaireListCommunicationPackage>(
                    "GetAllTemplates", 
                    cancellationToken, 
                    null, 
                    remoteUser.UserName,
                    remoteUser.Password, 
                    "GET");
            
        }

        public async Task<QuestionnaireCommunicationPackage> GetTemplateForCurrentUser(UserInfo remoteUser, Guid id, CancellationToken cancellationToken)
        {
            var webExecutor = restUtilsFactory.CreateRestServiceWrapper(CapiTesterApplication.GetPathToDesigner());
            return await webExecutor.ExecuteRestRequestAsync<QuestionnaireCommunicationPackage>(
                    "GetTemplate", 
                    cancellationToken, 
                    null, 
                    remoteUser.UserName,
                    remoteUser.Password, 
                    "GET",
                    new KeyValuePair<string, object>("id", id),
                    new KeyValuePair<string, object>("maxSupportedVersion", QuestionnaireVersionProvider.GetCurrentEngineVersion().ToString()));
                
        }
    }
}