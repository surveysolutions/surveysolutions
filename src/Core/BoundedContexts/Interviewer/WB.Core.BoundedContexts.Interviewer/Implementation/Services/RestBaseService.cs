using System;
using System.Net;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public abstract class RestBaseService
    {
        private readonly ILogger logger;

        protected RestBaseService(ILogger logger)
        {
            this.logger = logger;
        }

        protected async Task TryGetRestResponseOrThrowAsync(Func<Task> restRequestTask)
        {
            if (restRequestTask == null) throw new ArgumentNullException("restRequestTask");

            try
            {
                await restRequestTask();
            }
            catch (RestException ex)
            {
                throw this.CreateSynchronizationExceptionByRestException(ex);
            }
        }

        protected async Task<T> TryGetRestResponseOrThrowAsync<T>(Func<Task<T>> restRequestTask)
        {
            if (restRequestTask == null) throw new ArgumentNullException("restRequestTask");

            try
            {
                return await restRequestTask();
            }
            catch (RestException ex)
            {
                throw this.CreateSynchronizationExceptionByRestException(ex);
            }
        }

        protected SynchronizationException CreateSynchronizationExceptionByRestException(RestException ex)
        {
            string exceptionMessage = InterviewerUIResources.UnexpectedException;
            SynchronizationExceptionType exceptionType = SynchronizationExceptionType.Unexpected;
            switch (ex.Type)
            {
                case RestExceptionType.RequestByTimeout:
                    exceptionMessage = InterviewerUIResources.RequestTimeout;
                    exceptionType = SynchronizationExceptionType.RequestByTimeout;
                    break;
                case RestExceptionType.RequestCanceledByUser:
                    exceptionMessage = InterviewerUIResources.RequestCanceledByUser;
                    exceptionType = SynchronizationExceptionType.RequestCanceledByUser;
                    break;
                case RestExceptionType.HostUnreachable:
                    exceptionMessage = InterviewerUIResources.HostUnreachable;
                    exceptionType = SynchronizationExceptionType.HostUnreachable;
                    break;
                case RestExceptionType.InvalidUrl:
                    exceptionMessage = InterviewerUIResources.InvalidEndpoint;
                    exceptionType = SynchronizationExceptionType.InvalidUrl;
                    break;
                case RestExceptionType.NoNetwork:
                    exceptionMessage = InterviewerUIResources.NoNetwork;
                    exceptionType = SynchronizationExceptionType.NoNetwork;
                    break;
                case RestExceptionType.Unexpected:
                    switch (ex.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            if (ex.Message.Contains("lock"))
                            {
                                exceptionMessage = InterviewerUIResources.AccountIsLockedOnServer;
                                exceptionType = SynchronizationExceptionType.UserLocked;
                            }
                            else if (ex.Message.Contains("not approved"))
                            {
                                exceptionMessage = InterviewerUIResources.AccountIsNotApprovedOnServer;
                                exceptionType = SynchronizationExceptionType.UserNotApproved;
                            }
                            else if (ex.Message.Contains("not have an interviewer role"))
                            {
                                exceptionMessage = InterviewerUIResources.AccountIsNotAnInterviewer;
                                exceptionType = SynchronizationExceptionType.UserIsNotInterviewer;
                            }
                            else
                            {
                                exceptionMessage = InterviewerUIResources.Unauthorized;
                                exceptionType = SynchronizationExceptionType.Unauthorized;
                            }
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            var isMaintenance = ex.Message.Contains("maintenance");

                            if (isMaintenance)
                            {
                                exceptionMessage = InterviewerUIResources.Maintenance;
                                exceptionType = SynchronizationExceptionType.Maintenance;
                            }
                            else
                            {
                                this.logger.Warn("Server error", ex);

                                exceptionMessage = InterviewerUIResources.ServiceUnavailable;
                                exceptionType = SynchronizationExceptionType.ServiceUnavailable;
                            }
                            break;
                        case HttpStatusCode.UpgradeRequired:
                            exceptionMessage = InterviewerUIResources.UpgradeRequired;
                            exceptionType = SynchronizationExceptionType.UpgradeRequired;
                            break;
                        case HttpStatusCode.NotFound:
                            this.logger.Warn("Server error", ex);

                            exceptionMessage = InterviewerUIResources.InvalidEndpoint;
                            exceptionType = SynchronizationExceptionType.InvalidUrl;
                            break;
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.Redirect:
                            exceptionMessage = InterviewerUIResources.InvalidEndpoint;
                            exceptionType = SynchronizationExceptionType.InvalidUrl;
                            break;
                        case HttpStatusCode.InternalServerError:
                            this.logger.Warn("Server error", ex);

                            exceptionMessage = InterviewerUIResources.InternalServerError;
                            exceptionType = SynchronizationExceptionType.InternalServerError;
                            break;
                    }
                    break;
            }

            return new SynchronizationException(exceptionType, exceptionMessage, ex);
        }
    }
}