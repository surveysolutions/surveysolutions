using System;
using Main.Core.Events;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.InterviewMetaInfo;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.ChangeLog
{
    public class FileChangeLogStore : IChangeLogStore
    {
        private const string ChangelogFolder = "Changelog";
        private readonly string changelogPath;
        private readonly IArchiveUtils archiver;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ISerializer serializer;
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;

        public FileChangeLogStore(
            IArchiveUtils archiver, 
            IFileSystemAccessor fileSystemAccessor,
            ISerializer serializer,
            IAsyncPlainStorage<InterviewView> interviewViewRepository,
            string environmentalPersonalFolderPath)
        {
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
            this.serializer = serializer;
            this.interviewViewRepository = interviewViewRepository;
            this.changelogPath = fileSystemAccessor.CombinePath(environmentalPersonalFolderPath, ChangelogFolder);
            if (!fileSystemAccessor.IsDirectoryExists(this.changelogPath))
            {
                fileSystemAccessor.CreateDirectory(this.changelogPath);
            }
        }

        public void SaveChangeset(AggregateRootEvent[] recordData, Guid recordId)
        {
            if(recordData.Length==0)
                return;
            var path = this.GetFileName(recordId);
            var eventSourceId = recordData[0].EventSourceId;

            var metaData = this.interviewViewRepository.GetById(eventSourceId.ToString());

            var syncItem = new SyncItem()
                {
                    Content = this.archiver.CompressString(this.serializer.Serialize(recordData)),
                    IsCompressed = true,
                    ItemType = SyncItemType.Interview,
                    MetaInfo = this.archiver.CompressString(this.serializer.Serialize(metaData)),
                    RootId = eventSourceId
                };
            this.fileSystemAccessor.WriteAllText(path, this.serializer.Serialize(syncItem));
        }

        public string GetChangesetContent(Guid recordId)
        {
            var path = this.GetFileName(recordId);
            if (!this.fileSystemAccessor.IsFileExists(path))
                return null;
            return this.fileSystemAccessor.ReadAllText(path);
        }

        public void DeleteDraftChangeSet(Guid recordId)
        {
            var path = this.GetFileName(recordId);
            if (this.fileSystemAccessor.IsFileExists(path))
                this.fileSystemAccessor.DeleteFile(path);
        }

        private string GetFileName(Guid publicKey)
        {
            return this.fileSystemAccessor.CombinePath(this.changelogPath,
                                          publicKey.ToString());
        }


        public string GetPathToBackupFile()
        {
            return this.changelogPath;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithCahngelog = this.fileSystemAccessor.CombinePath(path, ChangelogFolder);

            foreach (var file in this.fileSystemAccessor.GetFilesInDirectory(this.changelogPath))
            {
                this.fileSystemAccessor.DeleteFile(file);
            }

            if (!this.fileSystemAccessor.IsDirectoryExists(dirWithCahngelog))
                return;

            foreach (var file in this.fileSystemAccessor.GetFilesInDirectory(dirWithCahngelog))
                this.fileSystemAccessor.CopyFileOrDirectory(file, this.changelogPath);
        }
    }
}