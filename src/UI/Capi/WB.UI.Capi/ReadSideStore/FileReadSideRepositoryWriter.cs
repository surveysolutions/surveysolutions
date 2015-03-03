using System;
using System.Collections.Generic;
using System.IO;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.UI.Capi.ReadSideStore
{
    public class FileReadSideRepositoryWriter<TEntity> : IReadSideKeyValueStorage<TEntity>, IBackupable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IJsonUtils jsonUtils;
        private Dictionary<string, TEntity> memcache = new Dictionary<string, TEntity>();

        public FileReadSideRepositoryWriter(IJsonUtils jsonUtils)
        {
            this.jsonUtils = jsonUtils;
            if (!Directory.Exists(this.StoreDirPath))
                Directory.CreateDirectory(this.StoreDirPath);
        }

        protected string StoreDirPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), typeof (TEntity).Name); }
        }

        private string GetFileName(string id)
        {
            Guid parsedGuid;
            var fileNameWithoutDashes = Path.Combine(this.StoreDirPath, id);
            
            if (!Guid.TryParse(id, out parsedGuid))
            {
                return fileNameWithoutDashes;
            }
            // backward compatibility
            var fileNameWithDashes = Path.Combine(this.StoreDirPath, parsedGuid.ToString());
            return File.Exists(fileNameWithDashes)
                // CAPI has file with dashes in filename
                ? fileNameWithDashes  
                // New version will create files in new format if where is no files in old format
                : fileNameWithoutDashes;
        }

        public TEntity GetById(string id)
        {
            if (!this.memcache.ContainsKey(id))
            {
                var filePath = this.GetFileName(id);
                if (!File.Exists(filePath))
                    return null;
                this.memcache[id] = this.jsonUtils.Deserialize<TEntity>(File.ReadAllText(filePath));
            }
            return this.memcache[id];
        }

        public void Remove(string id)
        {
            if (this.memcache.ContainsKey(id))
                this.memcache.Remove(id);
            File.Delete(this.GetFileName(id));
        }

        public void Store(TEntity view, string id)
        {
            var path = this.GetFileName(id);
            File.WriteAllText(path, this.jsonUtils.Serialize(view));

            this.memcache[id] = view;
        }

        public string GetPathToBackupFile()
        {
            return this.StoreDirPath;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithReadSide = Path.Combine(path, typeof (TEntity).Name);
            foreach (var file in Directory.EnumerateFiles(this.StoreDirPath))
            {
                File.Delete(file);
            }

            if (!Directory.Exists(dirWithReadSide))
                return;

            foreach (var file in Directory.GetFiles(dirWithReadSide))
                File.Copy(file, Path.Combine(this.StoreDirPath, Path.GetFileName(file)));
            this.memcache = new Dictionary<string, TEntity>();
        }
    }
}