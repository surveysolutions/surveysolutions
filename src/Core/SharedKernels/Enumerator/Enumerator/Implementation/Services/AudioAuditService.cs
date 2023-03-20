using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using Xamarin.Essentials;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class AudioAuditService : IAudioAuditService
    {
        private readonly IAudioService audioAuditService;
        private string fileNamePrefix = "audio-audit";
        private readonly IAudioAuditFileStorage audioAuditFileStorage;
        private readonly IPermissionsService permissions;
        public AudioAuditService(
            IAudioService audioAuditService, 
            IAudioAuditFileStorage audioAuditFileStorage, 
            IPermissionsService permissions,
            IFileSystemAccessor fileSystemAccessor,
            ILogger logger)
        {
            this.audioAuditService = audioAuditService;
            this.audioAuditFileStorage = audioAuditFileStorage;
            this.permissions = permissions;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
        }

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;

        public async Task StartAudioRecordingAsync(Guid interviewId)
        {
            await this.permissions.AssureHasPermissionOrThrow<Permissions.Microphone>().ConfigureAwait(false);
            await this.permissions.AssureHasExternalStoragePermissionOrThrow().ConfigureAwait(false);

            audioAuditService.StartAuditRecording($"{interviewId.FormatGuid()}-{fileNamePrefix}");
        }

        public void StopAudioRecording(Guid interviewId)
        {
            try
            {
                audioAuditService.StopAuditRecording();
            }
            catch (Exception e)
            {
                logger.Trace("Exception during stop of audio recording", e);
            }

            CheckAndProcessAllAuditFiles();
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
                            fileSystemAccessor.GetFileName(file),
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
