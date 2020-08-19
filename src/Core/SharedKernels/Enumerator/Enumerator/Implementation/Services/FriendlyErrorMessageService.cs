using System.Net;

using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;

using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class FriendlyErrorMessageService : IFriendlyErrorMessageService
    {
        readonly ILogger logger;

        public FriendlyErrorMessageService(ILogger logger)
        {
            this.logger = logger;
        }

        public string GetFriendlyErrorMessageByRestException(RestException ex)
        {
            switch (ex.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    if (ex.Message.Contains("lock")) return UIResources.ErrorMessage_AccountIsLockedOnServer;
                    if (ex.Message.Contains("not approved")) return UIResources.ErrorMessage_AccountIsNotApprovedOnServer;
                    return UIResources.ErrorMessage_Unauthorized;

                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.NotAcceptable:
                    var isMaintenance = ex.Message.Contains("maintenance");

                    if (!isMaintenance) this.logger.Warn("Server error", ex);

                    return isMaintenance ? UIResources.ErrorMessage_Maintenance : UIResources.ErrorMessage_ServiceUnavailable;

                case HttpStatusCode.RequestTimeout:
                    return UIResources.ErrorMessage_RequestTimeout;

                case HttpStatusCode.UpgradeRequired:
                    return UIResources.ErrorMessage_UpgradeRequired;

                case HttpStatusCode.NotFound:
                    this.logger.Warn("Server error", ex);
                    return UIResources.ErrorMessage_InvalidEndpoint;

                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Redirect:
                    return UIResources.ErrorMessage_InvalidEndpoint;

                case HttpStatusCode.InternalServerError:
                    this.logger.Warn("Server error", ex);
                    return UIResources.ErrorMessage_InternalServerError;
            }

            return null;
        }
    }
}