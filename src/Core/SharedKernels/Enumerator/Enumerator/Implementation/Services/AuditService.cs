using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAudioAuditService audioAuditService;
        private string fileNamePrefix = "audio-audit";
        private readonly IAudioAuditFileStorage audioAuditFileStorage;
        private readonly IPermissionsService permissions;
        public AuditService(
            IAudioAuditService audioAuditService, 
            IAudioAuditFileStorage audioAuditFileStorage, 
            IPermissionsService permissions)
        {
            this.audioAuditService = audioAuditService;
            this.audioAuditFileStorage = audioAuditFileStorage;
            this.permissions = permissions;
        }

        private string currentAuditFileName = null;
        private string currentAuditFilePath = null;

        public async Task StartAudioRecordingAsync(Guid interviewId)
        {
            Debug.WriteLine("!!!!!!!!!!! StartRecording");

            await this.permissions.AssureHasPermission(Permission.Microphone);
            await this.permissions.AssureHasPermission(Permission.Storage);
            
            currentAuditFileName = $"{fileNamePrefix}-{interviewId.FormatGuid()}-{DateTime.Now:yyyyMMdd_HHmmss}";
            try
            {
                currentAuditFilePath = audioAuditService.StartRecording(currentAuditFileName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        public Task StopAudioRecordingAsync(Guid interviewId)
        {
            Debug.WriteLine("!!!!!!!!!!! StopRecording");

            audioAuditService.StopRecording(currentAuditFileName);
            var audioStream = audioAuditService.GetRecord(currentAuditFilePath);
            var mimeType = this.audioAuditService.GetMimeType();
            using (var audioMemoryStream = new MemoryStream())
            {
                audioStream.CopyTo(audioMemoryStream);
                this.audioAuditFileStorage.StoreInterviewBinaryData(
                    interviewId, 
                    currentAuditFileName,
                    audioMemoryStream.ToArray(), 
                    mimeType);
            }

            currentAuditFileName = null;
            currentAuditFilePath = null;

            return Task.CompletedTask;
        }
    }
}
