using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public abstract class AbstractSynchronizationProcess
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private readonly IUserInteractionService userInteractionService;
        private readonly IInterviewerPrincipal principal;
        private IHttpStatistician httpStatistician;
        private readonly IPasswordHasher passwordHasher;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAuditLogService auditLogService;

        protected bool remoteLoginRequired;
        protected bool shouldUpdatePasswordOfInterviewer;
        protected RestCredentials restCredentials;

        protected abstract bool SendStatistics { get; }

        protected abstract string SucsessDescription { get; }

        protected AbstractSynchronizationProcess(
            ISynchronizationService synchronizationService, 
            ILogger logger,
            IHttpStatistician httpStatistician, 
            IUserInteractionService userInteractionService, 
            IInterviewerPrincipal principal,
            IPasswordHasher passwordHasher, 
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IAuditLogService auditLogService)
        {
            this.logger = logger;
            this.synchronizationService = synchronizationService;
            this.httpStatistician = httpStatistician;
            this.userInteractionService = userInteractionService;
            this.principal = principal;
            this.passwordHasher = passwordHasher;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.auditLogService = auditLogService;
        }


        public abstract Task Synchronize(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken,
            SynchronizationStatistics statistics);

        public abstract SyncStatisticsApiView ToSyncStatisticsApiView(SynchronizationStatistics statistics,
            Stopwatch stopwatch);

        protected Task<string> GetNewPasswordAsync()
        {
            var message =
                InterviewerUIResources.Synchronization_UserPassword_Update_Format.FormatString(this.principal
                    .CurrentUserIdentity.Name);
            return this.userInteractionService.ConfirmWithTextInputAsync(
                message,
                okButton: UIResources.LoginText,
                cancelButton: InterviewerUIResources.Synchronization_Cancel,
                isTextInputPassword: true);
        }

        protected async Task TrySendUnexpectedExceptionToServerAsync(Exception exception,
            CancellationToken cancellationToken)
        {
            try
            {
                await this.synchronizationService.SendUnexpectedExceptionAsync(
                    this.ToUnexpectedExceptionApiView(exception),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.Error("Synchronization. Exception when send exception to server", ex);
            }
        }

        private void UpdateSupervisorOfInterviewer(Guid supervisorId)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            localInterviewer.SupervisorId = supervisorId;
            this.interviewersPlainStorage.Store(localInterviewer);
            this.principal.SignInWithHash(localInterviewer.Name, localInterviewer.PasswordHash, true);
        }


        private void UpdatePasswordOfInterviewer(RestCredentials credentials)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            localInterviewer.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localInterviewer.Token = credentials.Token;

            this.interviewersPlainStorage.Store(localInterviewer);
            this.principal.SignIn(localInterviewer.Name, credentials.Password, true);
        }

        private UnexpectedExceptionApiView ToUnexpectedExceptionApiView(Exception exception)
        {
            return new UnexpectedExceptionApiView
            {
                Message = exception.Message,
                StackTrace = string.Join(Environment.NewLine,
                    exception.UnwrapAllInnerExceptions().Select(ex => $"{ex.Message} {ex.StackTrace}"))
            };
        }

        protected DeviceInfoApiView ToDeviceInfoApiView(DeviceInfo info)
        {
            return new DeviceInfoApiView
            {
                DeviceId = info.DeviceId,
                DeviceModel = info.DeviceModel,
                DeviceType = info.DeviceType,
                DeviceDate = info.DeviceDate,
                DeviceLanguage = info.DeviceLanguage,
                DeviceLocation =
                    info.DeviceLocation != null ? this.ToLocationAddressApiView(info.DeviceLocation) : null,
                DeviceManufacturer = info.DeviceManufacturer,
                DeviceBuildNumber = info.DeviceBuildNumber,
                DeviceSerialNumber = info.DeviceSerialNumber,

                AndroidVersion = info.AndroidVersion,
                AndroidSdkVersion = info.AndroidSdkVersion,
                AndroidSdkVersionName = info.AndroidSdkVersionName,

                AppVersion = info.AppVersion,
                AppBuildVersion = info.AppBuildVersion,
                AppOrientation = info.AppOrientation,
                LastAppUpdatedDate = info.LastAppUpdatedDate,

                BatteryChargePercent = info.BatteryChargePercent,
                BatteryPowerSource = info.BatteryPowerSource,
                IsPowerInSaveMode = info.IsPowerInSaveMode,

                MobileOperator = info.MobileOperator,
                MobileSignalStrength = info.MobileSignalStrength,
                NetworkType = info.NetworkType,
                NetworkSubType = info.NetworkSubType,

                NumberOfStartedInterviews = this.interviewViewRepository.Count(
                    interview => interview.StartedDateTime != null && interview.CompletedDateTime == null),

                DBSizeInfo = info.DBSizeInfo,
                StorageInfo = this.ToStorageInfoApiView(info.StorageInfo),
                RAMInfo = this.ToRAMInfoApiView(info.RAMInfo)
            };
        }

        private LocationAddressApiView ToLocationAddressApiView(LocationAddress locationAddress)
        {
            return new LocationAddressApiView
            {
                Longitude = locationAddress.Longitude,
                Latitude = locationAddress.Latitude
            };
        }

        private RAMInfoApiView ToRAMInfoApiView(RAMInfo ramInfo)
        {
            return new RAMInfoApiView
            {
                Free = ramInfo.Free,
                Total = ramInfo.Total
            };
        }

        private StorageInfoApiView ToStorageInfoApiView(StorageInfo storageInfo)
        {
            return new StorageInfoApiView
            {
                Free = storageInfo.Free,
                Total = storageInfo.Total
            };
        }

        public async Task SyncronizeAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            var statistics = new SynchronizationStatistics();
            try
            {
                auditLogService.Write(new SynchronizationStartedAuditLogEntity());

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                this.httpStatistician.Reset();

                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_UserAuthentication_Title,
                    Description = InterviewerUIResources.Synchronization_UserAuthentication_Description,
                    Status = SynchronizationStatus.Started,
                    Statistics = statistics
                });

                this.restCredentials = this.restCredentials ?? new RestCredentials
                {
                    Login = this.principal.CurrentUserIdentity.Name,
                    Token = this.principal.CurrentUserIdentity.Token
                };

                if (this.remoteLoginRequired)
                {
                    var token = await this.synchronizationService.LoginAsync(new LogonInfo
                    {
                        Username = this.restCredentials.Login,
                        Password = this.restCredentials.Password
                    }, this.restCredentials);

                    this.restCredentials.Password = this.restCredentials.Password;
                    this.restCredentials.Token = token;

                    this.remoteLoginRequired = false;

                    await this.synchronizationService.GetInterviewerAsync(this.restCredentials);
                }

                if (this.shouldUpdatePasswordOfInterviewer)
                {
                    this.shouldUpdatePasswordOfInterviewer = false;
                    this.UpdatePasswordOfInterviewer(this.restCredentials);
                }

                await this.synchronizationService.CanSynchronizeAsync(token: cancellationToken, credentials: this.restCredentials);

                var currentSupervisorId = await this.synchronizationService.GetCurrentSupervisor(token: cancellationToken, credentials: this.restCredentials);

                if (currentSupervisorId != this.principal.CurrentUserIdentity.SupervisorId)
                {
                    this.UpdateSupervisorOfInterviewer(currentSupervisorId);
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (SendStatistics)
                {
                    try
                    {
                        DeviceInfo deviceInfo = null;

                        using (var deviceInformationService =
                            ServiceLocator.Current.GetInstance<IDeviceInformationService>())
                        {
                            deviceInfo = await deviceInformationService.GetDeviceInfoAsync();
                        }

                        await this.synchronizationService.SendDeviceInfoAsync(this.ToDeviceInfoApiView(deviceInfo),
                            cancellationToken);
                    }
                    catch (Exception e)
                    {
                        await this.TrySendUnexpectedExceptionToServerAsync(e, cancellationToken);
                    }
                }

                await Synchronize(progress, cancellationToken, statistics);

                cancellationToken.ThrowIfCancellationRequested();

                if (SendStatistics)
                {
                    try
                    {
                        await this.synchronizationService.SendSyncStatisticsAsync(
                            this.ToSyncStatisticsApiView(statistics, stopwatch),
                            cancellationToken, this.restCredentials);
                    }
                    catch (Exception e)
                    {
                        await this.TrySendUnexpectedExceptionToServerAsync(e, cancellationToken);
                    }
                }

                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Success_Title,
                    Description = SucsessDescription, 
                    Status = SynchronizationStatus.Success,
                    Statistics = statistics
                });
            }
            catch (OperationCanceledException)
            {
                progress.Report(new SyncProgressInfo
                {
                    Status = SynchronizationStatus.Stopped,
                    Statistics = statistics
                });

                auditLogService.Write(new SynchronizationCanceledAuditLogEntity());

                return;
            }
            catch (SynchronizationException ex)
            {
                var errorTitle = InterviewerUIResources.Synchronization_Fail_Title;
                var errorDescription = ex.Message;

                auditLogService.Write(new SynchronizationFailedAuditLogEntity(ex));

                switch (ex.Type)
                {
                    case SynchronizationExceptionType.RequestCanceledByUser:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = errorTitle,
                            Description = errorDescription,
                            Status = SynchronizationStatus.Canceled,
                            Statistics = statistics
                        });

                        break;
                    case SynchronizationExceptionType.Unauthorized:
                        this.shouldUpdatePasswordOfInterviewer = true;
                        break;
                    case SynchronizationExceptionType.UserLinkedToAnotherDevice:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = InterviewerUIResources.Synchronization_UserLinkedToAnotherDevice_Status,
                            Description = InterviewerUIResources.Synchronization_UserLinkedToAnotherDevice_Title,
                            UserIsLinkedToAnotherDevice = true,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics
                        });
                        break;
                    case SynchronizationExceptionType.UnacceptableSSLCertificate:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = InterviewerUIResources.UnexpectedException,
                            Description = InterviewerUIResources.UnacceptableSSLCertificate,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics
                        });
                        break;
                    default:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = errorTitle,
                            Description = errorDescription,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Fail_Title,
                    Description = InterviewerUIResources.Synchronization_Fail_UnexpectedException,
                    Status = SynchronizationStatus.Fail,
                    Statistics = statistics
                });

                await this.TrySendUnexpectedExceptionToServerAsync(ex, cancellationToken);

                auditLogService.Write(new SynchronizationFailedAuditLogEntity(ex));

                this.logger.Error("Synchronization. Unexpected exception", ex);
            }

            if (!cancellationToken.IsCancellationRequested && this.shouldUpdatePasswordOfInterviewer)
            {
                var newPassword = await this.GetNewPasswordAsync();
                if (newPassword == null)
                {
                    this.shouldUpdatePasswordOfInterviewer = false;
                    progress.Report(new SyncProgressInfo
                    {
                        Title = InterviewerUIResources.Synchronization_Fail_Title,
                        Description = InterviewerUIResources.Unauthorized,
                        Status = SynchronizationStatus.Fail,
                        Statistics = statistics
                    });
                }
                else
                {
                    this.remoteLoginRequired = true;
                    this.restCredentials.Password = newPassword;
                    await this.SyncronizeAsync(progress, cancellationToken);
                }
            }
        }
    }
}
