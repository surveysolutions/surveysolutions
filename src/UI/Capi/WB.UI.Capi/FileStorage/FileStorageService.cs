using System.IO;

using WB.Core.Infrastructure.Backup;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.UI.Capi.FileStorage
{
    public class FileStorageService : IFileStorageService, IBackupable
    {
        private const string ImageFolder = "IMAGES";
        private readonly string _basePath;

        public FileStorageService()
        {
            this._basePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), ImageFolder);
            if (!Directory.Exists(this._basePath))
            {
                Directory.CreateDirectory(this._basePath);
            }
        }


        public void DeleteFile(string filename)
        {
            File.Delete(this.BuildFileName(filename));
        }

        public FileDescription RetrieveFile(string filename)
        {
            var longFileName = this.BuildFileName(filename);
            if (!File.Exists(longFileName))
                return null;
            var file = new FileDescription();

            file.Content = File.OpenRead(this.BuildFileName(filename));
            file.FileName = filename;
            //     file.Description = a.Metadata["Description"].Value<string>();
            //    file.Title = a.Metadata["Description"].Value<string>();
            return file;
        }

        public FileDescription RetrieveThumb(string filename)
        {
            return this.RetrieveFile(filename);
        }

        public void StoreFile(FileDescription file)
        {
            var longFileName = this.BuildFileName(file.FileName);
            if (File.Exists(longFileName))
                this.DeleteFile(file.FileName);
            using (var o = File.Open(
                this.BuildFileName(file.FileName),
                FileMode.Create)
                )
            {
                byte[] buf = new byte[1024];
                int r;
                while ((r = file.Content.Read(buf, 0, buf.Length)) > 0)
                    o.Write(buf, 0, r);
            }
        }

        private string BuildFileName(string fileName)
        {
            return Path.Combine(this._basePath, fileName);
        }

        public string GetPathToBackupFile()
        {
            return this._basePath;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithImages = Path.Combine(path, ImageFolder);
            foreach (var file in Directory.EnumerateFiles(this._basePath))
            {
                File.Delete(file);
            }

            if (!Directory.Exists(dirWithImages))
                return;

            foreach (var file in Directory.GetFiles(dirWithImages))
                File.Copy(file, Path.Combine(this._basePath, Path.GetFileName(file)));
        }
    }
}