#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public abstract class AbstractSynchronizationProcess: ISynchronizationProcess
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private readonly IPrincipal principal;
        private readonly IHttpStatistician httpStatistician;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        protected readonly IAuditLogService auditLogService;
        private readonly IEnumeratorSettings enumeratorSettings;
        private readonly IServiceLocator serviceLocator;
        private readonly IDeviceInformationService deviceInformationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IAssignmentDocumentsStorage assignmentsStorage;

        private bool remoteLoginRequired;
        private bool shouldUpdatePasswordOfResponsible;
        private bool changePasswordRequired;
        protected RestCredentials? RestCredentials;
        
        protected AbstractSynchronizationProcess(
            ISynchronizationService synchronizationService, 
            ILogger logger,
            IHttpStatistician httpStatistician,
            IPrincipal principal,
            IPlainStorage<InterviewView> interviewViewRepository,
            IAuditLogService auditLogService, 
            IEnumeratorSettings enumeratorSettings,
            IServiceLocator serviceLocator,
            IDeviceInformationService deviceInformationService,
            IUserInteractionService userInteractionService,
            IAssignmentDocumentsStorage assignmentsStorage)
        {
            this.logger = logger;
            this.synchronizationService = synchronizationService;
            this.httpStatistician = httpStatistician;
            this.principal = principal;
            this.interviewViewRepository = interviewViewRepository;
            this.auditLogService = auditLogService;
            this.enumeratorSettings = enumeratorSettings;
            this.serviceLocator = serviceLocator;
            this.deviceInformationService = deviceInformationService;
            this.userInteractionService = userInteractionService;
            this.assignmentsStorage = assignmentsStorage;
        }

        public virtual async Task Synchronize(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken,
            SynchronizationStatistics statistics)
        {
            var steps = this.serviceLocator.GetAllInstances<ISynchronizationStep>();

            var context = new EnumeratorSynchonizationContext
            {
                Progress = progress,
                CancellationToken = cancellationToken,
                Statistics = statistics
            };

            foreach (var step in steps.OrderBy(x => x.SortOrder))
            {
                cancellationToken.ThrowIfCancellationRequested();
                step.Context = context;
                this.logger.Trace($"Executing synchronization step {step.GetType().Name}");
                await step.ExecuteAsync().ConfigureAwait(false);
            }
        }

        public async Task ForceUpdateAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken,
            SynchronizationStatistics statistics)
        {
            var updateAppStep = this.serviceLocator.GetInstance<IUpdateApplicationSynchronizationStep>();

            var context = new EnumeratorSynchonizationContext
            {
                Progress = progress,
                CancellationToken = cancellationToken,
                Statistics = statistics
            };

            cancellationToken.ThrowIfCancellationRequested();
            updateAppStep.Context = context;
            await updateAppStep.ExecuteAsync();
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
                    exception.UnwrapAllInnerExceptions().Select(ex => $"{ex.Message}{Environment.NewLine}{ex.StackTrace}"))
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
                this.enumeratorSettings.MarkSyncStart();

                this.WriteToAuditLogStartSyncMessage();

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                this.httpStatistician.Reset();

                progress.Report(new SyncProgressInfo
                {
                    Title = EnumeratorUIResources.Synchronization_UserAuthentication_Title,
                    Description = EnumeratorUIResources.Synchronization_UserAuthentication_Description,
                    Status = SynchronizationStatus.Started,
                    Statistics = statistics,
                    Stage = SyncStage.UserAuthentication
                });

                this.RestCredentials ??= new RestCredentials
                {
                    Login = this.principal.CurrentUserIdentity.Name,
                    Token = this.principal.CurrentUserIdentity.Token,
                    Workspace = this.principal.CurrentUserIdentity.Workspace
                };

                bool shouldUpdateCredentialsInDb = false;

                if (this.remoteLoginRequired)
                {
                    this.remoteLoginRequired = false;

                    var token = await this.synchronizationService.LoginAsync(new LogonInfo
                    {
                        Username = this.RestCredentials.Login,
                        Password = this.RestCredentials.Password
                    }, this.RestCredentials, cancellationToken).ConfigureAwait(false);

                    this.RestCredentials.Password = this.RestCredentials.Password;
                    this.RestCredentials.Token = token;

                    shouldUpdateCredentialsInDb = true;
                }

                if (shouldUpdateCredentialsInDb)
                {
                    this.UpdatePasswordOfResponsible(this.RestCredentials);
                }

                await CanSynchronizeAsync(progress, cancellationToken, statistics).ConfigureAwait(false);

                await CheckAfterStartSynchronization(cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                if (SendStatistics)
                {
                    try
                    {
                        var deviceInfo = await deviceInformationService.GetDeviceInfoAsync().ConfigureAwait(false);

                        await this.synchronizationService.SendDeviceInfoAsync(this.ToDeviceInfoApiView(deviceInfo),
                            cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        await this.TrySendUnexpectedExceptionToServerAsync(e);
                    }
                }

                await Synchronize(progress, cancellationToken, statistics).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                if (SendStatistics)
                {
                    try
                    {
                        var hqTimestamp = await this.synchronizationService.SendSyncStatisticsAsync(
                            this.ToSyncStatisticsApiView(statistics, stopwatch),
                            this.RestCredentials, cancellationToken).ConfigureAwait(false);

                        this.enumeratorSettings.SetLastHqSyncTimestamp(hqTimestamp);

                        OnSuccessfulSynchronization();
                    }
                    catch (Exception e)
                    {
                        await this.TrySendUnexpectedExceptionToServerAsync(e).ConfigureAwait(false);
                    }
                }

                this.enumeratorSettings.MarkSyncSucceeded();

                progress.Report(new SyncProgressInfo
                {
                    Title = EnumeratorUIResources.Synchronization_Success_Title,
                    Description = SuccessDescription,
                    Status = SynchronizationStatus.Success,
                    Statistics = statistics,
                    Stage = SyncStage.Success
                });
            }
            catch (OperationCanceledException)
            {
                progress.Report(new SyncProgressInfo
                {
                    Status = SynchronizationStatus.Stopped,
                    Statistics = statistics,
                    Stage = SyncStage.Stopped
                });

                auditLogService.Write(new SynchronizationCanceledAuditLogEntity());

                return;
            }
            catch (SynchronizationException ex)
            {
                var errorTitle = EnumeratorUIResources.Synchronization_Fail_Title;
                var errorDescription = ex.Message;

                switch (ex.Type)
                {
                    case SynchronizationExceptionType.RequestCanceledByUser:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = errorTitle,
                            Description = errorDescription,
                            Status = SynchronizationStatus.Canceled,
                            Statistics = statistics,
                            Stage = SyncStage.Canceled
                        });
                        auditLogService.Write(new SynchronizationCanceledAuditLogEntity());
                        break;
                    case SynchronizationExceptionType.Unauthorized:
                        this.shouldUpdatePasswordOfResponsible = true;
                        break;
                    case SynchronizationExceptionType.ShouldChangePassword:
                        this.changePasswordRequired = true;
                        break;
                    case SynchronizationExceptionType.UserLocked:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = errorTitle,
                            Description = EnumeratorUIResources.AccountIsLockedOnServer,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics,
                            Stage = SyncStage.FailedAccountIsLockedOnServer
                        });
                        break;
                    case SynchronizationExceptionType.UserLinkedToAnotherDevice:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = EnumeratorUIResources.Synchronization_UserLinkedToAnotherDevice_Status,
                            Description = EnumeratorUIResources.Synchronization_UserLinkedToAnotherDevice_Title,
                            UserIsLinkedToAnotherDevice = true,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics,
                            Stage = SyncStage.FailedUserLinkedToAnotherDevice
                        });
                        auditLogService.Write(SynchronizationFailedAuditLogEntity.CreateFromException(ex));
                        break;
                    case SynchronizationExceptionType.UserLinkedToAnotherServer:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = EnumeratorUIResources.Synchronization_UserLinkedToAnotherServer_Status,
                            Description = EnumeratorUIResources.Synchronization_UserLinkedToAnotherServer_Description,
                            UserIsLinkedToAnotherDevice = true,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics,
                            Stage = SyncStage.FailedUserLinkedToAnotherDevice
                        });
                        auditLogService.Write(SynchronizationFailedAuditLogEntity.CreateFromException(ex));
                        break;
                    case SynchronizationExceptionType.SupervisorRequireOnlineSync:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = EnumeratorUIResources.Synchronization_SupervisorShouldDoOnlineSync_Title,
                            Description = EnumeratorUIResources.Synchronization_SupervisorShouldDoOnlineSync,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics,
                            Stage = SyncStage.FailedSupervisorShouldDoOnlineSync
                        });
                        auditLogService.Write(SynchronizationFailedAuditLogEntity.CreateFromException(ex));
                        break;
                    case SynchronizationExceptionType.UnacceptableSSLCertificate:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = EnumeratorUIResources.UnexpectedException,
                            Description = EnumeratorUIResources.UnacceptableSSLCertificate,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics,
                            Stage = SyncStage.FailedUnacceptableSSLCertificate

                        });
                        auditLogService.Write(SynchronizationFailedAuditLogEntity.CreateFromException(ex));
                        break;
                    case SynchronizationExceptionType.InterviewerFromDifferentTeam:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = EnumeratorUIResources.UnexpectedException,
                            Description = EnumeratorUIResources.Synchronization_UserDoNotBelongToTeam,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics,
                            Stage = SyncStage.FailedUserDoNotBelongToTeam
                        });
                        auditLogService.Write(SynchronizationFailedAuditLogEntity.CreateFromException(ex));
                        break;

                    case SynchronizationExceptionType.UpgradeRequired:
                        var message = EnumeratorUIResources.UpgradeRequired;

                        var targetVersionObj = ex.Data["target-version"];
                        if (targetVersionObj != null && targetVersionObj is string targetVersion)
                        {
                            var appVersionName = deviceInformationService.GetApplicationVersionName();
                            message = GetRequiredUpdate(targetVersion, appVersionName);
                        }

                        progress.Report(new SyncProgressInfo
                        {
                            Title = message,
                            Status = SynchronizationStatus.Fail,
                            IsApplicationUpdateRequired = true,
                            Statistics = statistics,
                            Stage = SyncStage.FailedUpgradeRequired
                        });
                        break;
                    case SynchronizationExceptionType.WorkspaceDisabled:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = EnumeratorUIResources.Synchronization_UserLinkedToAnotherDevice_Status,
                            Description = EnumeratorUIResources.Synchronization_WorkspaceDisabled,
                            Status = SynchronizationStatus.Fail,
                            Stage = SyncStage.UserAuthentication
                        });
                        break;
                    case SynchronizationExceptionType.WorkspaceAccessDisabledReason:
                        progress.Report(new SyncProgressInfo
                        {
                            Description = EnumeratorUIResources.Synchronization_WorkspaceAccessDisabledReason,
                            Status = SynchronizationStatus.Fail,
                            Stage = SyncStage.UserAuthentication
                        });
                        break;
                    default:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = errorTitle,
                            Description = errorDescription,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics,
                            Stage = SyncStage.Failed
                        });
                        auditLogService.Write(SynchronizationFailedAuditLogEntity.CreateFromException(ex));
                        break;
                }
            }
            catch (MissingPermissionsException ex)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = EnumeratorUIResources.Synchronization_Fail_Title,
                    Description = ex.Message,
                    Status = SynchronizationStatus.Fail,
                    Statistics = statistics,
                    Stage = SyncStage.Failed
                });
            }
            catch (ActiveWorkspaceRemovedException we)
            {
                var workspace = principal.CurrentUserIdentity.Workspace;
                this.logger.Error($"Workspace {workspace} removed.", we);

                await ChangeAndNavigateToNewDefaultWorkspaceAsync();
            }
            catch (Exception ex)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = EnumeratorUIResources.Synchronization_Fail_Title,
                    Description = EnumeratorUIResources.Synchronization_Fail_UnexpectedException,
                    Status = SynchronizationStatus.Fail,
                    Statistics = statistics,
                    Stage = SyncStage.FailedUnexpectedException
                });

                await this.TrySendUnexpectedExceptionToServerAsync(ex).ConfigureAwait(false);

                auditLogService.Write(SynchronizationFailedAuditLogEntity.CreateFromException(ex));

                this.logger.Error("Synchronization. Unexpected exception", ex);
            }

            if (!cancellationToken.IsCancellationRequested && this.shouldUpdatePasswordOfResponsible)
            {
                this.shouldUpdatePasswordOfResponsible = false;

                var newPassword = await this.GetNewPasswordAsync().ConfigureAwait(false);
                if (newPassword == null)
                {
                    progress.Report(new SyncProgressInfo
                    {
                        Title = EnumeratorUIResources.Synchronization_Fail_Title,
                        Description = EnumeratorUIResources.Unauthorized,
                        Status = SynchronizationStatus.Fail,
                        Statistics = statistics,
                        Stage = SyncStage.FailedUnauthorized
                    });
                }
                else
                {
                    this.remoteLoginRequired = true;
                    if (this.RestCredentials != null)
                    {
                        this.RestCredentials.Password = newPassword;
                    }

                    await this.SynchronizeAsync(progress, cancellationToken).ConfigureAwait(false);
                }
            }
            
            if (!cancellationToken.IsCancellationRequested && this.changePasswordRequired)
            {
                this.changePasswordRequired = false;

                var password = await this.GetNewChangedPasswordAsync().ConfigureAwait(false);
                if (password == null)
                {
                    progress.Report(new SyncProgressInfo
                    {
                        Title = EnumeratorUIResources.Synchronization_Fail_Title,
                        Description = EnumeratorUIResources.Unauthorized,
                        Status = SynchronizationStatus.Fail,
                        Statistics = statistics,
                        Stage = SyncStage.FailedUnauthorized
                    });
                }
                else
                {
                    await this.SynchronizeAsync(progress, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task CanSynchronizeAsync(IProgress<SyncProgressInfo> progress, 
            CancellationToken cancellationToken,
            SynchronizationStatistics statistics)
        {
            try
            {
                await this.synchronizationService.CanSynchronizeAsync(this.RestCredentials, this.principal.CurrentUserIdentity.TenantId,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (SynchronizationException ex) when (ex.Type == SynchronizationExceptionType.UpgradeRequired)
            {
                await ForceUpdateAsync(progress, cancellationToken, statistics);

                throw;
            }
        }
        
        protected virtual void OnSuccessfulSynchronization() { }

        protected abstract Task CheckAfterStartSynchronization(CancellationToken cancellationToken);
        protected abstract Task ChangeAndNavigateToNewDefaultWorkspaceAsync();
        protected abstract void UpdatePasswordOfResponsible(RestCredentials credentials);

        protected abstract string GetRequiredUpdate(string targetVersion, string appVersion);

        protected virtual Task<string?> GetNewPasswordAsync()
        {
            var message = EnumeratorUIResources.Synchronization_UserPassword_Update_Format.FormatString(
                this.principal.CurrentUserIdentity.Name);

            return this.userInteractionService.ConfirmWithTextInputAsync(
                message,
                okButton: UIResources.LoginText,
                cancelButton: EnumeratorUIResources.Synchronization_Cancel,
                isTextInputPassword: true);
        }
        
        protected virtual async Task<ChangePasswordDialogResult?> GetNewChangedPasswordAsync()
        {
            var message = EnumeratorUIResources.Synchronization_PasswordChangeRequired;

            var dialogResult = await this.userInteractionService.ConfirmNewPasswordInputAsync(
                message,
                okCallback: ChangePasswordCallback,
                okButton: UIResources.Ok,
                cancelButton: EnumeratorUIResources.Synchronization_Cancel);
            
            return dialogResult;
        }
        
        private async Task ChangePasswordCallback(ChangePasswordDialogOkCallback callback)
        {
            if (callback.DialogResult != null)
            {
                var username = principal.CurrentUserIdentity.Name;
                var changePasswordInfo = new ChangePasswordInfo
                {
                    Username = username,
                    Password = callback.DialogResult.OldPassword,
                    NewPassword = callback.DialogResult.NewPassword,
                };

                try
                {
                    var token = await this.synchronizationService.ChangePasswordAsync(changePasswordInfo)
                        .ConfigureAwait(false);

                    if (this.RestCredentials != null && !string.IsNullOrWhiteSpace(token))
                    {
                        this.RestCredentials.Token = token;
                        this.RestCredentials.Password = changePasswordInfo.NewPassword;

                        this.UpdatePasswordOfResponsible(this.RestCredentials);
                        return;
                    }
                }
                catch (SynchronizationException ex)
                {
                    if (ex.Type == SynchronizationExceptionType.ShouldChangePassword)
                        callback.NewPasswordError = ex.Message;
                    else
                        callback.OldPasswordError = ex.Message;
                    
                    logger.Error($"Cant change password for user {username}", ex);
                }
            }

            callback.NeedClose = false;
        }
        
        protected virtual void WriteToAuditLogStartSyncMessage()
        {
            this.auditLogService.Write(new SynchronizationStartedAuditLogEntity(SynchronizationType.Online));
        }

        protected virtual void WriteToAuditLog(IAuditLogEntity entry)
        {
            this.auditLogService.Write(entry);
        }

        protected virtual bool SendStatistics => true;
        protected virtual string SuccessDescription => EnumeratorUIResources.Synchronization_Success_Description;

        public virtual SyncStatisticsApiView ToSyncStatisticsApiView(SynchronizationStatistics statistics, Stopwatch stopwatch)
        {
            var httpStats = this.httpStatistician.GetStats();

            return new SyncStatisticsApiView
            {
                DownloadedInterviewsCount = statistics.NewInterviewsCount,
                UploadedInterviewsCount = statistics.SuccessfullyUploadedInterviewsCount,
                DownloadedQuestionnairesCount = statistics.SuccessfullyDownloadedQuestionnairesCount,
                RejectedInterviewsOnDeviceCount =
                    this.interviewViewRepository.Count(
                        inteview => inteview.Status == InterviewStatus.RejectedBySupervisor),
                NewInterviewsOnDeviceCount =
                    this.interviewViewRepository.Count(
                        inteview => inteview.Status == InterviewStatus.InterviewerAssigned && !inteview.CanBeDeleted),
                RemovedInterviewsCount = statistics.DeletedInterviewsCount,

                NewAssignmentsCount = statistics.NewAssignmentsCount,
                RemovedAssignmentsCount = statistics.RemovedAssignmentsCount,
                AssignmentsOnDeviceCount = this.assignmentsStorage.Count(),

                TotalDownloadedBytes = httpStats.DownloadedBytes,
                TotalUploadedBytes = httpStats.UploadedBytes,
                TotalConnectionSpeed = httpStats.Speed,
                TotalSyncDuration = stopwatch.Elapsed
            };
        }
    }
}
