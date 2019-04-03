﻿using System;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorSynchronizeHandler : IHandleCommunicationMessage
    {
        private readonly IPlainStorage<InterviewerDocument> interviewerViewRepository;
        private readonly ISupervisorSettings settings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ISecureStorage secureStorage;

        public SupervisorSynchronizeHandler(
            IPlainStorage<InterviewerDocument> interviewerViewRepository,
            ISupervisorSettings settings,
            IFileSystemAccessor fileSystemAccessor,
            ISecureStorage secureStorage)
        {
            this.interviewerViewRepository = interviewerViewRepository;
            this.settings = settings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.secureStorage = secureStorage;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<CanSynchronizeRequest, CanSynchronizeResponse>(CanSynchronize);
            requestHandler.RegisterHandler<GetLatestApplicationVersionRequest, GetLatestApplicationVersionResponse>(GetLatestApplicationVersion);
            requestHandler.RegisterHandler<GetPublicKeyForEncryptionRequest, GetPublicKeyForEncryptionResponse>(GetPublicKeyForEncryptionAsync);
        }

        private Task<GetPublicKeyForEncryptionResponse> GetPublicKeyForEncryptionAsync(GetPublicKeyForEncryptionRequest request)
        {
            var response = new GetPublicKeyForEncryptionResponse
            {
                PublicKey = this.secureStorage.Contains(RsaEncryptionService.PublicKey)
                    ? Convert.ToBase64String(this.secureStorage.Retrieve(RsaEncryptionService.PublicKey))
                    : null
            };
            return Task.FromResult(response);
        }

        private Task<GetLatestApplicationVersionResponse> GetLatestApplicationVersion(
            GetLatestApplicationVersionRequest request)
        {
            var sAppVersion = this.settings.GetApplicationVersionCode().ToString();

            var pathToInterviewerApks = this.fileSystemAccessor.CombinePath(
                this.settings.InterviewerApplicationsDirectory, sAppVersion);

            var hasApks = this.fileSystemAccessor.IsDirectoryExists(pathToInterviewerApks) &&
                          this.fileSystemAccessor.GetFilesInDirectory(pathToInterviewerApks)
                              .Count(f => f.EndsWith(".apk")) == 2;//.Length == 2;

            return Task.FromResult(new GetLatestApplicationVersionResponse
            {
                InterviewerApplicationVersion = hasApks
                    ? this.settings.GetApplicationVersionCode()
                    : (int?) null
            });
        }

        public async Task<CanSynchronizeResponse> CanSynchronize(CanSynchronizeRequest arg)
        {
            if (settings.LastHqSyncTimestamp == null ||
                arg.LastHqSyncTimestamp != null && arg.LastHqSyncTimestamp > settings.LastHqSyncTimestamp)
            {
                return new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.SupervisorRequireOnlineSync
                };
            }

            var supervisorBuildNumber = this.settings.GetApplicationVersionCode();
            if (arg.InterviewerBuildNumber > supervisorBuildNumber)
            {
                return new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.UnexpectedClientVersion
                };
            }

            if (arg.InterviewerBuildNumber < supervisorBuildNumber)
            {
                // check that supervisor has apks for interviewers
                var latestInterviewerVersion = await GetLatestApplicationVersion(new GetLatestApplicationVersionRequest());
                if (!latestInterviewerVersion.InterviewerApplicationVersion.HasValue)
                {
                    return new CanSynchronizeResponse
                    {
                        CanSyncronize = false,
                        Reason = SyncDeclineReason.UnexpectedClientVersion
                    };
                }
            }

            var user = interviewerViewRepository.GetById(arg.InterviewerId.FormatGuid());
            if (user == null)
            {
                return new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.NotATeamMember
                };
            }

            if (user.IsLockedByHeadquarters || user.IsLockedBySupervisor)
            {
                return new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.UserIsLocked
                };
            }

            return new CanSynchronizeResponse
            {
                CanSyncronize = true
            };
        }
    }
}
