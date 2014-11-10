using System;
using System.IO;
using Main.Core;
using Main.Core.Events;
using Main.Core.View;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.InterviewMetaInfo;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.BoundedContexts.Capi.Synchronization.Implementation.ChangeLog
{
    public class FileChangeLogStore : IChangeLogStore
    {
        private const string ChangelogFolder = "Changelog";
        private readonly string changelogPath;
        private readonly IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo> metaInfoFactory;
        private readonly IArchiveUtils archiver;

        public FileChangeLogStore(IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo> metaInfoFactory, IArchiveUtils archiver)
        {
            this.metaInfoFactory = metaInfoFactory;
            this.archiver = archiver;
            this.changelogPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), ChangelogFolder);
            if (!Directory.Exists(this.changelogPath))
            {
                Directory.CreateDirectory(this.changelogPath);
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
                    Content = archiver.CompressString(JsonUtils.GetJsonData(recordData)),
                    IsCompressed = true,
                    ItemType = SyncItemType.Questionnare,
                    MetaInfo = archiver.CompressString(
                        JsonUtils.GetJsonData(
                            metaData)),
                    Id = eventSourceId
                };
            File.WriteAllText(path, JsonUtils.GetJsonData(syncItem));
        }

        public string GetChangesetContent(Guid recordId)
        {
            var path = this.GetFileName(recordId);
            if (!File.Exists(path))
                return null;
            return File.ReadAllText(path);
        }

        public void DeleteDraftChangeSet(Guid recordId)
        {
            var path = this.GetFileName(recordId);
            if (File.Exists(path))
                File.Delete(path);
        }

        private string GetFileName(Guid publicKey)
        {
            return System.IO.Path.Combine(this.changelogPath,
                                          publicKey.ToString());
        }


        public string GetPathToBackupFile()
        {
            return this.changelogPath;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithCahngelog = Path.Combine(path, ChangelogFolder);
            
            foreach (var file in Directory.EnumerateFiles(this.changelogPath))
            {
                File.Delete(file);
            }

            if (!Directory.Exists(dirWithCahngelog))
                return;

            foreach (var file in Directory.GetFiles(dirWithCahngelog))
                File.Copy(file, Path.Combine(this.changelogPath, Path.GetFileName(file)));
        }
    }
}