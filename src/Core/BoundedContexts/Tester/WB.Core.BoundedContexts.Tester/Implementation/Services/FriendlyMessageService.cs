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
            switch (ex.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    return ex.Message.Contains("lock") ? UIResources.AccountIsLockedOnServer : UIResources.Unauthorized;

                case HttpStatusCode.ServiceUnavailable:
                    var isMaintenance = ex.Message.Contains("maintenance");

                    if (!isMaintenance) this.logger.Warn("Server error", ex);

                    return isMaintenance ? UIResources.Maintenance : UIResources.ServiceUnavailable;

                case HttpStatusCode.RequestTimeout:
                    return UIResources.RequestTimeout;

                case HttpStatusCode.UpgradeRequired:
                    return UIResources.ImportQuestionnaire_Error_UpgradeRequired;

                case HttpStatusCode.NotFound:
                    this.logger.Warn("Server error", ex);
                    return UIResources.InvalidEndpoint;

                case HttpStatusCode.BadRequest:
                    return UIResources.InvalidEndpoint;

                case HttpStatusCode.InternalServerError:
                    this.logger.Warn("Server error", ex);
                    return UIResources.InternalServerError;
            }

            return null;
        }
    }
}