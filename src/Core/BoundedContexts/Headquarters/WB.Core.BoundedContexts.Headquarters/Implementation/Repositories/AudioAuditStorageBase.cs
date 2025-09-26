using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public abstract class AudioAuditStorageBase : IAudioAuditFileStorage
    {
        protected virtual string GetFileId(Guid interviewId, string fileName) => $"{interviewId}#{fileName}";
        
        public abstract Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId);
        public abstract byte[] GetInterviewBinaryData(Guid interviewId, string fileName);
        public abstract Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName);
        public abstract Task RemoveInterviewBinaryData(Guid interviewId, string fileName);
        public abstract void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType);
        public abstract Task<bool> HasAnyAudioAuditFilesStoredAsync(QuestionnaireIdentity questionnaire);
    }
}
