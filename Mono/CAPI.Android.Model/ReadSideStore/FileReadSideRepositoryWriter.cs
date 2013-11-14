using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using Environment = System.Environment;

namespace CAPI.Android.Core.Model.ReadSideStore
{
    public class FileReadSideRepositoryWriter<TEntity> : IReadSideRepositoryWriter<TEntity>, IBackupable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private Dictionary<Guid, TEntity> memcache = new Dictionary<Guid, TEntity>();
        public FileReadSideRepositoryWriter()
        {
            if (!Directory.Exists(StoreDirPath))
                Directory.CreateDirectory(StoreDirPath);
        }

        protected string StoreDirPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), typeof (TEntity).Name); }
        }

        private string GetFileName(Guid id)
        {
            return Path.Combine(StoreDirPath, id.ToString());
        }

        public TEntity GetById(Guid id)
        {
            if (!memcache.ContainsKey(id))
            {
                var filePath = GetFileName(id);
                if (!File.Exists(filePath))
                    return null;
                memcache[id] = JsonUtils.GetObject<TEntity>(File.ReadAllText(filePath));
            }
            return memcache[id];
        }

        public void Remove(Guid id)
        {
            if (memcache.ContainsKey(id))
                memcache.Remove(id);
            File.Delete(GetFileName(id));
        }

        public void Store(TEntity view, Guid id)
        {
            var path = GetFileName(id);
            File.WriteAllText(path, JsonUtils.GetJsonData(view));

            memcache[id] = view;
        }

        public string GetPathToBakupFile()
        {
            return StoreDirPath;
        }

        public void RestoreFromBakupFolder(string path)
        {
            foreach (var file in Directory.EnumerateFiles(StoreDirPath))
            {
                File.Delete(file);
            }

            var dirWithReadSide = Path.Combine(path, typeof (TEntity).Name);
            if (!Directory.Exists(dirWithReadSide))
                return;


            foreach (var file in Directory.GetFiles(dirWithReadSide))
                File.Copy(file, Path.Combine(StoreDirPath, Path.GetFileName(file)));
            memcache = new Dictionary<Guid, TEntity>();
        }
    }
}