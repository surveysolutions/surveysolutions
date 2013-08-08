using System;
using System.IO;
using CAPI.Android.Core.Model.ModelUtils;
using CAPI.Android.Core.Model.ViewModel.InterviewMetaInfo;
using Main.Core;
using Main.Core.Events;
using Main.Core.View;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Core.Model.ChangeLog
{
    public class FileChangeLogStore : IChangeLogStore
    {
        private const string ChangelogFolder = "Changelog";
        private readonly string changelogPath;
        private readonly IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo> metaInfoFactory;

        public FileChangeLogStore(IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo> metaInfoFactory)
        {
            this.metaInfoFactory = metaInfoFactory;
            changelogPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), ChangelogFolder);
            if (!Directory.Exists(changelogPath))
            {
                Directory.CreateDirectory(changelogPath);
            }
        }

        public void SaveChangeset(AggregateRootEvent[] recordData, Guid recordId)
        {
            if(recordData.Length==0)
                return;
            var path = GetFileName(recordId);
            var eventSourceId = recordData[0].EventSourceId;
            var syncItem = new SyncItem()
                {
                    Content = PackageHelper.CompressString(JsonUtils.GetJsonData(recordData)),
                    IsCompressed = true,
                    ItemType = SyncItemType.Questionnare,
                    MetaInfo =
                        PackageHelper.CompressString(
                            JsonUtils.GetJsonData(
                                metaInfoFactory.Load(new InterviewMetaInfoInputModel(eventSourceId)))),
                    Id = eventSourceId
                };
            File.WriteAllText(path, JsonUtils.GetJsonData(syncItem));
        }

        public string GetChangesetContent(Guid recordId)
        {
            var path = GetFileName(recordId);
            if (!File.Exists(path))
                return null;
            return File.ReadAllText(path);
        }

        public void DeleteDraftChangeSet(Guid recordId)
        {
            var path = GetFileName(recordId);
            if (File.Exists(path))
                File.Delete(path);
        }

        private string GetFileName(Guid publicKey)
        {
            return System.IO.Path.Combine(changelogPath,
                                          publicKey.ToString());
        }


        public string GetPathToBakupFile()
        {
            return changelogPath;
        }

        public void RestoreFromBakupFolder(string path)
        {
            var dirWithCahngelog = Path.Combine(path, ChangelogFolder);
            foreach (var file in Directory.EnumerateFiles(changelogPath))
            {
                File.Delete(file);
            }

            foreach (var file in Directory.GetFiles(dirWithCahngelog))
                File.Copy(file, Path.Combine(changelogPath, Path.GetFileName(file)));
        }
    }
}