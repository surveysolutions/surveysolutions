using WB.Core.BoundedContexts.Headquarters.DesignerPublicService;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation
{
    internal class DesignerService : IDesignerService
    {
        public void TryLogin(string userName, string password)
        {
            var service = new PublicServiceClient();
            service.ClientCredentials.UserName.UserName = userName;
            service.ClientCredentials.UserName.Password = password;
            service.Dummy();
        }
    }
}