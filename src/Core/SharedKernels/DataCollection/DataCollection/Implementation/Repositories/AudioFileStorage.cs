using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    public class AudioFile : IView
    {
        public virtual string Id { get; set; }

        public virtual Guid InterviewId { get; set; }

        public virtual string FileName { get; set; }

        public virtual byte[] Data { get; set; }

        public virtual string ContentType { get; set; }

        public static string GetFileId(Guid interviewId, string fileName) => $"{interviewId}#{fileName}";
    }

    public class AudioFileStorage : IAudioFileStorage
    {
        private readonly IPlainStorageAccessor<AudioFile> filePlainStorageAccessor;

        public AudioFileStorage(IPlainStorageAccessor<AudioFile> filePlainStorageAccessor)
        {
            this.filePlainStorageAccessor = filePlainStorageAccessor;
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileId = AudioFile.GetFileId(interviewId, fileName);
            var databaseFile = filePlainStorageAccessor.GetById(fileId);
            return databaseFile?.Data;
        }

        public List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId)
        {
            var databaseFiles = filePlainStorageAccessor.Query(q => q.Where(f => f.InterviewId == interviewId));

            return databaseFiles.Select(file 
                => new InterviewBinaryDataDescriptor(
                    interviewId, 
                    file.FileName,
                    () => GetInterviewBinaryData(interviewId, file.FileName)
                )).ToList();
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            var file = new AudioFile()
            {
                Id = AudioFile.GetFileId(interviewId, fileName),
                InterviewId = interviewId,
                FileName = fileName,
                Data = data,
                ContentType = contentType
            };
            var fileId = AudioFile.GetFileId(interviewId, fileName);

            filePlainStorageAccessor.Store(file, fileId);
        }

        public void RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileId = AudioFile.GetFileId(interviewId, fileName);
            filePlainStorageAccessor.Remove(fileId);
        }
    }
}
