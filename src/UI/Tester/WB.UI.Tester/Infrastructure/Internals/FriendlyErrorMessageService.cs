using System.Net;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Services;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Tester.Infrastructure.Internals
{
    internal class FriendlyErrorMessageService : IFriendlyErrorMessageService
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
                    if (ex.Message.Contains("lock")) return UIResources.AccountIsLockedOnServer;
                    if (ex.Message.Contains("not approved")) return UIResources.AccountIsNotApprovedOnServer;
                    return UIResources.Unauthorized;

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
                case HttpStatusCode.Redirect:
                    return UIResources.InvalidEndpoint;

                case HttpStatusCode.InternalServerError:
                    this.logger.Warn("Server error", ex);
                    return UIResources.InternalServerError;
            }

            return null;
        }
    }
}