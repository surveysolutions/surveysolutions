using System;
using Main.Core.Events;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Views.InterviewMetaInfo;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.BoundedContexts.Capi.Implementation.ChangeLog
{
    public class FileChangeLogStore : IChangeLogStore
    {
        private const string ChangelogFolder = "Changelog";
        private readonly string changelogPath;
        private readonly IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo> metaInfoFactory;
        private readonly IArchiveUtils archiver;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IJsonUtils jsonUtils;

        public FileChangeLogStore(
            IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo> metaInfoFactory,
            IArchiveUtils archiver, 
            IFileSystemAccessor fileSystemAccessor,
            IJsonUtils jsonUtils,
            string environmentalPersonalFolderPath)
        {
            this.metaInfoFactory = metaInfoFactory;
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
            this.jsonUtils = jsonUtils;
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

            var metaData = this.metaInfoFactory.Load(new InterviewMetaInfoInputModel(eventSourceId));

            var syncItem = new SyncItem()
                {
                    Content = this.archiver.CompressString(this.jsonUtils.Serialize(recordData)),
                    IsCompressed = true,
                    ItemType = SyncItemType.Interview,
                    MetaInfo = this.archiver.CompressString(this.jsonUtils.Serialize(metaData)),
                    RootId = eventSourceId
                };
            fileSystemAccessor.WriteAllText(path, this.jsonUtils.Serialize(syncItem));
        }

        public string GetChangesetContent(Guid recordId)
        {
            var path = this.GetFileName(recordId);
            if (!fileSystemAccessor.IsFileExists(path))
                return null;
            return fileSystemAccessor.ReadAllText(path);
        }

        public void DeleteDraftChangeSet(Guid recordId)
        {
            var path = this.GetFileName(recordId);
            if (fileSystemAccessor.IsFileExists(path))
                fileSystemAccessor.DeleteFile(path);
        }

        private string GetFileName(Guid publicKey)
        {
            return fileSystemAccessor.CombinePath(this.changelogPath,
                                          publicKey.ToString());
        }


        public string GetPathToBackupFile()
        {
            return this.changelogPath;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithCahngelog = fileSystemAccessor.CombinePath(path, ChangelogFolder);

            foreach (var file in fileSystemAccessor.GetFilesInDirectory(this.changelogPath))
            {
                fileSystemAccessor.DeleteFile(file);
            }

            if (!fileSystemAccessor.IsDirectoryExists(dirWithCahngelog))
                return;

            foreach (var file in fileSystemAccessor.GetFilesInDirectory(dirWithCahngelog))
                fileSystemAccessor.CopyFileOrDirectory(file, this.changelogPath);
        }
    }
}