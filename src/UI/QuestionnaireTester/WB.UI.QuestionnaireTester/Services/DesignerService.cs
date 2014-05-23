using System;
using System.Collections.Generic;
using System.Threading;
using Ninject;
using WB.Core.GenericSubdomains.Rest;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.UI.QuestionnaireTester.Services
{
    public class DesignerService
    {
        private readonly IRestServiceWrapperFactory restUtilsFactory;

        public DesignerService()
        {
            restUtilsFactory = CapiTesterApplication.Kernel.Get<IRestServiceWrapperFactory>();
        }

        public bool Login(string userName, string password, CancellationToken cancellationToken)
        {
            var webExecutor = restUtilsFactory.CreateRestServiceWrapper(CapiTesterApplication.GetPathToDesigner());
            try
            {
                return webExecutor.ExecuteRestRequestAsync<bool>(
                    "ValidateCredentials", cancellationToken, null, userName, password, "POST");
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public QuestionnaireListCommunicationPackage  GetQuestionnaireListForCurrentUser(CancellationToken cancellationToken)
        {
            if (!CapiTesterApplication.DesignerMembership.IsLoggedIn)
                return null;
            var webExecutor = restUtilsFactory.CreateRestServiceWrapper(CapiTesterApplication.GetPathToDesigner());
            try
            {
                return webExecutor.ExecuteRestRequestAsync<QuestionnaireListCommunicationPackage>(
                    "GetAllTemplates", cancellationToken, null, CapiTesterApplication.DesignerMembership.RemoteUser.UserName,
                    CapiTesterApplication.DesignerMembership.RemoteUser.Password, "GET");
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public QuestionnaireCommunicationPackage GetTemplateForCurrentUser(Guid id, CancellationToken cancellationToken)
        {
            if (!CapiTesterApplication.DesignerMembership.IsLoggedIn)
                return null;

            var webExecutor = restUtilsFactory.CreateRestServiceWrapper(CapiTesterApplication.GetPathToDesigner());
            try
            {
                var package = webExecutor.ExecuteRestRequestAsync<QuestionnaireCommunicationPackage>(
                    "GetTemplate", cancellationToken, null, CapiTesterApplication.DesignerMembership.RemoteUser.UserName,
                    CapiTesterApplication.DesignerMembership.RemoteUser.Password, "GET",
                    new KeyValuePair<string, string>("id", id.ToString()));

                return package;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}