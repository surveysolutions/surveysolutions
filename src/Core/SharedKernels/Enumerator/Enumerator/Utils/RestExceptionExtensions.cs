using System.Net;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public static class RestExceptionExtensions
    {
        public static SynchronizationException ToSynchronizationException(this RestException restException)
        {
            string exceptionMessage = InterviewerUIResources.UnexpectedException;
            SynchronizationExceptionType exceptionType = SynchronizationExceptionType.Unexpected;
            switch (restException.Type)
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
                case RestExceptionType.UnacceptableCertificate:
                    exceptionMessage = InterviewerUIResources.UnacceptableSSLCertificate;
                    exceptionType = SynchronizationExceptionType.UnacceptableSSLCertificate;
                    break;
                case RestExceptionType.Unexpected:
                    switch (restException.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            if (restException.Message.Contains("lock"))
                            {
                                exceptionMessage = InterviewerUIResources.AccountIsLockedOnServer;
                                exceptionType = SynchronizationExceptionType.UserLocked;
                            }
                            else if (restException.Message.Contains("not approved"))
                            {
                                exceptionMessage = InterviewerUIResources.AccountIsNotApprovedOnServer;
                                exceptionType = SynchronizationExceptionType.UserNotApproved;
                            }
                            else if (restException.Message.Contains("not have an interviewer role"))
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
                            var isMaintenance = restException.Message.Contains("maintenance");

                            if (isMaintenance)
                            {
                                exceptionMessage = InterviewerUIResources.Maintenance;
                                exceptionType = SynchronizationExceptionType.Maintenance;
                            }
                            else
                            {
                                exceptionMessage = InterviewerUIResources.ServiceUnavailable;
                                exceptionType = SynchronizationExceptionType.ServiceUnavailable;
                            }
                            break;
                        case HttpStatusCode.NotAcceptable:
                            exceptionMessage = InterviewerUIResources.NotSupportedServerSyncProtocolVersion;
                            exceptionType = SynchronizationExceptionType.NotSupportedServerSyncProtocolVersion;
                            break;
                        case HttpStatusCode.UpgradeRequired:
                            exceptionMessage = InterviewerUIResources.UpgradeRequired;
                            exceptionType = SynchronizationExceptionType.UpgradeRequired;
                            break;
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.Redirect:
                            exceptionMessage = InterviewerUIResources.InvalidEndpoint;
                            exceptionType = SynchronizationExceptionType.InvalidUrl;
                            break;
                        case HttpStatusCode.NotFound:
                            exceptionMessage = InterviewerUIResources.InvalidEndpoint;
                            exceptionType = SynchronizationExceptionType.InvalidUrl;
                            break;
                        case HttpStatusCode.InternalServerError:

                            exceptionMessage = InterviewerUIResources.InternalServerError;
                            exceptionType = SynchronizationExceptionType.InternalServerError;
                            break;
                        case HttpStatusCode.Forbidden:
                            exceptionType = SynchronizationExceptionType.UserLinkedToAnotherDevice;
                            break;
                    }
                    break;
            }

            return new SynchronizationException(exceptionType, exceptionMessage, restException);
        }
    }
}
