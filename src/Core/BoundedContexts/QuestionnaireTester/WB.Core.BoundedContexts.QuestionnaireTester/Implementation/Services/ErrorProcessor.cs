using System;
using System.Net;

using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    public class ErrorProcessor : IErrorProcessor
    {
        private readonly ILogger logger;

        public ErrorProcessor(ILogger logger)
        {
            this.logger = logger;
        }

        public TesterError GetInternalErrorAndLogException(Exception exception, TesterHttpAction action)
        {
            if (exception is RestException)
            {
                var restException = (RestException)exception;
                switch (restException.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return restException.Message.Contains("lock")
                           ? new TesterError(ErrorCode.AccountIsLockedOnDesigner, UIResources.AccountIsLockedOnServer)
                           : new TesterError(ErrorCode.UserWasNotAuthoredOnDesigner, UIResources.Unauthorized);

                    case HttpStatusCode.ServiceUnavailable:
                        return restException.Message.Contains("maintenance")
                            ? new TesterError(ErrorCode.DesignerIsInMaintenanceMode, UIResources.Maintenance)
                            : new TesterError(ErrorCode.DesignerIsUnavailable, UIResources.ServiceUnavailable);

                    case HttpStatusCode.RequestTimeout:
                        return new TesterError(ErrorCode.RequestTimeout, UIResources.RequestTimeout);
                        
                    case HttpStatusCode.InternalServerError:
                        this.logger.Error("Internal server error when getting questionnaires.", restException);
                        return new TesterError(ErrorCode.InternalServerError, UIResources.InternalServerError);

                    case HttpStatusCode.NotFound:
                        this.logger.Error("Designer cannot be found.", restException);
                        if (action == TesterHttpAction.Login)
                        {
                            return new TesterError(ErrorCode.RequestedUrlWasNotFound, UIResources.Login_Error_NotFound);
                        }
                        return new TesterError(ErrorCode.RequestedUrlWasNotFound, UIResources.ImportQuestionnaire_Error_NotFound);

                    case HttpStatusCode.Forbidden:
                        return new TesterError(ErrorCode.RequestedUrlIsForbidden, UIResources.ImportQuestionnaire_Error_Forbidden);

                    case HttpStatusCode.UpgradeRequired:
                        return new TesterError(ErrorCode.TesterUpgradeIsRequiredToOpenQuestionnaire, UIResources.ImportQuestionnaire_Error_UpgradeRequired);

                    case HttpStatusCode.PreconditionFailed:
                        return new TesterError(ErrorCode.QuestionnaireContainsErrorsAndCannotBeOpened, UIResources.ImportQuestionnaire_Error_PreconditionFailed);

                    default:
                        return new TesterError(ErrorCode.UnknownError, UIResources.Maintenance);
                }
            }

            this.logger.Error("Internal server error when getting questionnaires.", exception);
            return new TesterError(ErrorCode.UnknownError, UIResources.InternalServerError);
        }
    }
}
