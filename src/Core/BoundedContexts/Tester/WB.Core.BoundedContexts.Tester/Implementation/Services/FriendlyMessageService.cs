using System.Net;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
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