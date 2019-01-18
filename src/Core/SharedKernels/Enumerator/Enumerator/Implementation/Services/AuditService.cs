﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAudioService audioAuditService;
        private string fileNamePrefix = "audio-audit";
        private readonly IAudioAuditFileStorage audioAuditFileStorage;
        private readonly IPermissionsService permissions;
        public AuditService(
            IAudioService audioAuditService, 
            IAudioAuditFileStorage audioAuditFileStorage, 
            IPermissionsService permissions,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.audioAuditService = audioAuditService;
            this.audioAuditFileStorage = audioAuditFileStorage;
            this.permissions = permissions;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        private readonly IFileSystemAccessor fileSystemAccessor;

        public async Task StartAudioRecordingAsync(Guid interviewId)
        {
            await this.permissions.AssureHasPermission(Permission.Microphone);
            await this.permissions.AssureHasPermission(Permission.Storage);

            audioAuditService.StartAuditRecording($"{interviewId.FormatGuid()}-{fileNamePrefix}");
        }

        public Task StopAudioRecordingAsync(Guid interviewId)
        {
            audioAuditService.StopAuditRecording();
            CheckAndProcessAllAuditFiles();

            return Task.CompletedTask;
        }

        private void ProcessAllFiles(string[] files)
        {
            foreach (var file in files)
            {
                try
                {
                    var audioStream = audioAuditService.GetRecord(file);
                    var mimeType = this.audioAuditService.GetMimeType();
                    using (var audioMemoryStream = new MemoryStream())
                    {
                        audioStream.CopyTo(audioMemoryStream);
                        this.audioAuditFileStorage.StoreInterviewBinaryData(
                            GetInterviewIdFromFileName(fileSystemAccessor.GetFileNameWithoutExtension(file)),
                            fileSystemAccessor.GetFileNameWithoutExtension(file),
                            audioMemoryStream.ToArray(),
                            mimeType);
                    }

                    fileSystemAccessor.DeleteFile(file);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    throw;
                }
            }
        }

        private Guid GetInterviewIdFromFileName(string filename)
        {
            string stringToCheck = String.Empty;

            if (!String.IsNullOrWhiteSpace(filename))
            {
                int charLocation = filename.IndexOf("-", StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    stringToCheck = filename.Substring(0, charLocation);
                }
            }

            return Guid.TryParse(stringToCheck, out var identifier) ? identifier : Guid.Empty;
        }

        public void CheckAndProcessAllAuditFiles()
        {
            var files = fileSystemAccessor.GetFilesInDirectory(audioAuditService.GetAuditPath());
            ProcessAllFiles(files);
        }
    }
}
