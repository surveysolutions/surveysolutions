using System.Net;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class FriendlyMessageService : IFriendlyMessageService
    {
        public string GetFriendlyErrorMessageByRestException(RestException ex)
        {
            switch (ex.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    return ex.Message.Contains("lock") ? UIResources.AccountIsLockedOnServer : UIResources.Unauthorized;

                case HttpStatusCode.ServiceUnavailable:
                    return ex.Message.Contains("maintenance") ? UIResources.Maintenance : UIResources.ServiceUnavailable;

                case HttpStatusCode.RequestTimeout:
                    return UIResources.RequestTimeout;

                case HttpStatusCode.UpgradeRequired:
                    return UIResources.ImportQuestionnaire_Error_UpgradeRequired;

                case HttpStatusCode.BadRequest:
                    return UIResources.InvalidEndpoint;

                case HttpStatusCode.InternalServerError:
                    return UIResources.InternalServerError;
            }

            return null;
        }
    }
}