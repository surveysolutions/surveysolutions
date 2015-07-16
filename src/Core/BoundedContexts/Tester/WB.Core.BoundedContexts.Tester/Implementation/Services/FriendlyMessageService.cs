using System.Net;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class FriendlyMessageService : IFriendlyMessageService
    {
        readonly ILogger logger;

        public FriendlyMessageService(ILogger logger)
        {
            this.logger = logger;
        }

        public string GetFriendlyErrorMessageByRestException(RestException ex)
        {
            bool shouldRestExceptionBeLogged = false;
            string friendlyMessage = null;

            switch (ex.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    friendlyMessage = ex.Message.Contains("lock")
                        ? UIResources.AccountIsLockedOnServer
                        : UIResources.Unauthorized;
                    break;
                case HttpStatusCode.ServiceUnavailable:
                    var isMaintenance = ex.Message.Contains("maintenance");

                    shouldRestExceptionBeLogged = !isMaintenance;

                    friendlyMessage = isMaintenance
                        ? UIResources.Maintenance
                        : UIResources.ServiceUnavailable;
                    break;
                case HttpStatusCode.RequestTimeout:
                    friendlyMessage = UIResources.RequestTimeout;
                    break;
                case HttpStatusCode.UpgradeRequired:
                    friendlyMessage = UIResources.ImportQuestionnaire_Error_UpgradeRequired;
                    break;
                case HttpStatusCode.NotFound:
                    shouldRestExceptionBeLogged = true;
                    friendlyMessage = UIResources.InvalidEndpoint;
                    break;
                case HttpStatusCode.BadRequest:
                    friendlyMessage = UIResources.InvalidEndpoint;
                    break;
                case HttpStatusCode.InternalServerError:
                    shouldRestExceptionBeLogged = true;
                    friendlyMessage = UIResources.InternalServerError;
                    break;
            }

            if (shouldRestExceptionBeLogged) this.logger.Warn("Server error", ex);

            return friendlyMessage;
        }
    }
}