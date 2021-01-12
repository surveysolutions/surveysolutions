using System.IO;
using System.Net;
using System.Net.Mime;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public static class RestExceptionExtensions
    {
        public static SynchronizationException ToSynchronizationException(this RestException restException)
        {
            string exceptionMessage = EnumeratorUIResources.UnexpectedException;
            SynchronizationExceptionType exceptionType = SynchronizationExceptionType.Unexpected;
            switch (restException.Type)
            {
                case RestExceptionType.RequestByTimeout:
                    exceptionMessage = EnumeratorUIResources.RequestTimeout;
                    exceptionType = SynchronizationExceptionType.RequestByTimeout;
                    break;
                case RestExceptionType.RequestCanceledByUser:
                    exceptionMessage = EnumeratorUIResources.RequestCanceledByUser;
                    exceptionType = SynchronizationExceptionType.RequestCanceledByUser;
                    break;
                case RestExceptionType.HostUnreachable:
                    exceptionMessage = EnumeratorUIResources.HostUnreachable;
                    exceptionType = SynchronizationExceptionType.HostUnreachable;
                    break;
                case RestExceptionType.InvalidUrl:
                    exceptionMessage = EnumeratorUIResources.InvalidEndpoint;
                    exceptionType = SynchronizationExceptionType.InvalidUrl;
                    break;
                case RestExceptionType.NoNetwork:
                    exceptionMessage = EnumeratorUIResources.NoNetwork;
                    exceptionType = SynchronizationExceptionType.NoNetwork;
                    break;
                case RestExceptionType.UnacceptableCertificate:
                    exceptionMessage = EnumeratorUIResources.UnacceptableSSLCertificate;
                    exceptionType = SynchronizationExceptionType.UnacceptableSSLCertificate;
                    break;
                case RestExceptionType.Unexpected:
                    switch (restException.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            if (restException.Message.Contains("lock"))
                            {
                                exceptionMessage = EnumeratorUIResources.AccountIsLockedOnServer;
                                exceptionType = SynchronizationExceptionType.UserLocked;
                            }
                            else if (restException.Message.Contains("not approved"))
                            {
                                exceptionMessage = EnumeratorUIResources.AccountIsNotApprovedOnServer;
                                exceptionType = SynchronizationExceptionType.UserNotApproved;
                            }
                            else if (restException.Message.Contains("not have an interviewer role"))
                            {
                                exceptionMessage = EnumeratorUIResources.AccountIsNotAnInterviewer;
                                exceptionType = SynchronizationExceptionType.UserIsNotInterviewer;
                            }
                            else
                            {
                                exceptionMessage = EnumeratorUIResources.Unauthorized;
                                exceptionType = SynchronizationExceptionType.Unauthorized;
                            }

                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            var isMaintenance = restException.Message.Contains("maintenance");

                            if (isMaintenance)
                            {
                                exceptionMessage = EnumeratorUIResources.Maintenance;
                                exceptionType = SynchronizationExceptionType.Maintenance;
                            }
                            else
                            {
                                exceptionMessage = EnumeratorUIResources.ServiceUnavailable;
                                exceptionType = SynchronizationExceptionType.ServiceUnavailable;
                            }

                            break;
                        case HttpStatusCode.NotAcceptable:
                            exceptionMessage = EnumeratorUIResources.NotSupportedServerSyncProtocolVersion;
                            exceptionType = SynchronizationExceptionType.NotSupportedServerSyncProtocolVersion;
                            break;
                        case HttpStatusCode.UpgradeRequired:
                            var exception = new SynchronizationException(
                                SynchronizationExceptionType.UpgradeRequired,
                                EnumeratorUIResources.UpgradeRequired);
                            if (restException.Data.Contains("target-version"))
                                exception.Data["target-version"] = restException.Data["target-version"];
                            return exception;
                        case HttpStatusCode.Conflict:
                            exceptionType = SynchronizationExceptionType.UserLinkedToAnotherServer;
                            break;
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.MethodNotAllowed:
                            exceptionMessage = EnumeratorUIResources.InvalidEndpoint;
                            exceptionType = SynchronizationExceptionType.InvalidUrl;
                            break;
                        case HttpStatusCode.NotFound:
                            exceptionMessage = EnumeratorUIResources.InvalidEndpoint;
                            exceptionType = SynchronizationExceptionType.InvalidUrl;
                            break;
                        case HttpStatusCode.InternalServerError:
                            exceptionMessage = EnumeratorUIResources.InternalServerError;
                            exceptionType = SynchronizationExceptionType.InternalServerError;
                            break;
                        case HttpStatusCode.Forbidden:
                        {
                            if (restException.Message.Contains("relinked"))
                            {
                                exceptionType = SynchronizationExceptionType.UserLinkedToAnotherDevice;
                                exceptionMessage =
                                    EnumeratorUIResources.Synchronization_UserLinkedToAnotherDevice_Title;
                            }else if(restException.Message.Contains("Workspace is disabled"))
                            {
                                exceptionType = SynchronizationExceptionType.WorkspaceDisabled;
                                exceptionMessage =
                                    EnumeratorUIResources.Synchronization_WorkspaceDisabled;
                            }
                            else
                            {
                                exceptionMessage = EnumeratorUIResources.Unauthorized;
                                exceptionType = SynchronizationExceptionType.Unauthorized;
                            }
                            break;
                        }
                    }

                    break;
            }

            return new SynchronizationException(exceptionType, exceptionMessage, restException);
        }
    }
}
