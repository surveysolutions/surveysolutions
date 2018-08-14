using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public abstract class AbstractSynchronizationProcess
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private readonly IUserInteractionService userInteractionService;
        private readonly IPrincipal principal;
        private readonly IHttpStatistician httpStatistician;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAuditLogService auditLogService;
        private readonly IEnumeratorSettings enumeratorSettings;

        private bool remoteLoginRequired;
        private bool shouldUpdatePasswordOfResponsible;
        protected RestCredentials RestCredentials;

        protected abstract bool SendStatistics { get; }

        protected abstract string SucsessDescription { get; }

        protected virtual SynchronizationType SynchronizationType => SynchronizationType.Online;

        protected AbstractSynchronizationProcess(
            ISynchronizationService synchronizationService, 
            ILogger logger,
            IHttpStatistician httpStatistician, 
            IUserInteractionService userInteractionService,
            IPrincipal principal,
            IPlainStorage<InterviewView> interviewViewRepository,
            IAuditLogService auditLogService, 
            IEnumeratorSettings enumeratorSettings)
        {
            this.logger = logger;
            this.synchronizationService = synchronizationService;
            this.httpStatistician = httpStatistician;
            this.userInteractionService = userInteractionService;
            this.principal = principal;
            this.interviewViewRepository = interviewViewRepository;
            this.auditLogService = auditLogService;
            this.enumeratorSettings = enumeratorSettings;
        }

        public abstract Task Synchronize(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken,
            SynchronizationStatistics statistics);

        public abstract SyncStatisticsApiView ToSyncStatisticsApiView(SynchronizationStatistics statistics,
            Stopwatch stopwatch);

        protected Task<string> GetNewPasswordAsync()
        {
            if (SynchronizationType == SynchronizationType.Offline)
                return Task.FromResult((string)null);

            var message =
                InterviewerUIResources.Synchronization_UserPassword_Update_Format.FormatString(this.principal
                    .CurrentUserIdentity.Name);
            return this.userInteractionService.ConfirmWithTextInputAsync(
                message,
                okButton: UIResources.LoginText,
                cancelButton: InterviewerUIResources.Synchronization_Cancel,
                isTextInputPassword: true);
        }

        protected async Task TrySendUnexpectedExceptionToServerAsync(Exception exception)
        {
            if (exception.GetSelfOrInnerAs<OperationCanceledException>() != null)
                return;

            try
            {
                await this.synchronizationService.SendUnexpectedExceptionAsync(
                    this.ToUnexpectedExceptionApiView(exception), CancellationToken.None);
            }
            catch (Exception ex)
            {
                this.logger.Error("Synchronization. Exception when send exception to server", ex);
            }
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

        public async Task SynchronizeAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            var statistics = new SynchronizationStatistics();
            try
            {
                auditLogService.Write(new SynchronizationStartedAuditLogEntity(SynchronizationType));

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

                this.RestCredentials = this.RestCredentials ?? new RestCredentials
                {
                    Login = this.principal.CurrentUserIdentity.Name,
                    Token = this.principal.CurrentUserIdentity.Token
                };

                if (this.remoteLoginRequired)
                {
                    var token = await this.synchronizationService.LoginAsync(new LogonInfo
                    {
                        Username = this.RestCredentials.Login,
                        Password = this.RestCredentials.Password
                    }, this.RestCredentials);

                    this.RestCredentials.Password = this.RestCredentials.Password;
                    this.RestCredentials.Token = token;

                    this.remoteLoginRequired = false;
                }

                if (this.shouldUpdatePasswordOfResponsible)
                {
                    this.shouldUpdatePasswordOfResponsible = false;
                    this.UpdatePasswordOfResponsible(this.RestCredentials);
                }

                await this.synchronizationService.CanSynchronizeAsync(this.RestCredentials, cancellationToken);

                await CheckAfterStartSynchronization(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (SendStatistics)
                {
                    try
                    {
                        DeviceInfo deviceInfo;

                        using (var deviceInformationService = ServiceLocator.Current.GetInstance<IDeviceInformationService>())
                        {
                            deviceInfo = await deviceInformationService.GetDeviceInfoAsync();
                        }

                        await this.synchronizationService.SendDeviceInfoAsync(this.ToDeviceInfoApiView(deviceInfo),
                            cancellationToken);
                    }
                    catch (Exception e)
                    {
                        await this.TrySendUnexpectedExceptionToServerAsync(e);
                    }
                }

                await Synchronize(progress, cancellationToken, statistics);

                cancellationToken.ThrowIfCancellationRequested();

                if (SendStatistics)
                {
                    try
                    {
                        var hqTimestamp = await this.synchronizationService.SendSyncStatisticsAsync(
                            this.ToSyncStatisticsApiView(statistics, stopwatch),
                            cancellationToken, this.RestCredentials);

                        this.enumeratorSettings.SetLastHqSyncTimestamp(hqTimestamp);

                        OnSuccesfullSynchronization();
                    }
                    catch (Exception e)
                    {
                        await this.TrySendUnexpectedExceptionToServerAsync(e);
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
                        auditLogService.Write(new SynchronizationCanceledAuditLogEntity());
                        break;
                    case SynchronizationExceptionType.Unauthorized:
                        this.shouldUpdatePasswordOfResponsible = true;
                        break;
                    case SynchronizationExceptionType.UserLocked:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = InterviewerUIResources.Synchronization_Fail_Title,
                            Description = InterviewerUIResources.AccountIsLockedOnServer,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics
                        });
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
                        auditLogService.Write(new SynchronizationFailedAuditLogEntity(ex));
                        break;
                    case SynchronizationExceptionType.SupervisorRequireOnlineSync:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = InterviewerUIResources.Synchronization_SupervisorShouldDoOnlineSync_Title,
                            Description = InterviewerUIResources.Synchronization_SupervisorShouldDoOnlineSync,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics
                        });
                        auditLogService.Write(new SynchronizationFailedAuditLogEntity(ex));
                        break;
                    case SynchronizationExceptionType.UnacceptableSSLCertificate:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = InterviewerUIResources.UnexpectedException,
                            Description = InterviewerUIResources.UnacceptableSSLCertificate,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics
                        });
                        auditLogService.Write(new SynchronizationFailedAuditLogEntity(ex));
                        break;
                    case SynchronizationExceptionType.InterviewerFromDifferentTeam:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = InterviewerUIResources.UnexpectedException,
                            Description = InterviewerUIResources.Synchronization_UserDoNotBelongToTeam,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics
                        });
                        auditLogService.Write(new SynchronizationFailedAuditLogEntity(ex));
                        break;

                    case SynchronizationExceptionType.UpgradeRequired:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = InterviewerUIResources.UpgradeRequired,
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
                        auditLogService.Write(new SynchronizationFailedAuditLogEntity(ex));
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

                await this.TrySendUnexpectedExceptionToServerAsync(ex);

                auditLogService.Write(new SynchronizationFailedAuditLogEntity(ex));

                this.logger.Error("Synchronization. Unexpected exception", ex);
            }

            if (!cancellationToken.IsCancellationRequested && this.shouldUpdatePasswordOfResponsible)
            {
                var newPassword = await this.GetNewPasswordAsync();
                if (newPassword == null)
                {
                    this.shouldUpdatePasswordOfResponsible = false;
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
                    this.RestCredentials.Password = newPassword;
                    await this.SynchronizeAsync(progress, cancellationToken);
                }
            }
        }

        protected virtual void OnSuccesfullSynchronization() { }

        protected abstract Task CheckAfterStartSynchronization(CancellationToken cancellationToken);

        protected abstract void UpdatePasswordOfResponsible(RestCredentials credentials);
    }
}
