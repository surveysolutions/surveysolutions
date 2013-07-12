using System.IO;
using Main.Core.Documents;
using Main.Core.Services;
using WB.Core.Infrastructure.Backup;

namespace CAPI.Android.Core.Model.FileStorage
{
    public class FileStorageService : IFileStorageService,IBackupable
    {
        private const string ImageFolder = "IMAGES";
        private readonly string _basePath;

        public FileStorageService()
        {
            _basePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), ImageFolder);
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }
        

        public void DeleteFile(string filename)
        {
            File.Delete(BuildFileName(filename));
        }

        public FileDescription RetrieveFile(string filename)
        {
            var longFileName = BuildFileName(filename);
            if (!File.Exists(longFileName))
                return null;
            var file = new FileDescription();

            file.Content = File.OpenRead(BuildFileName(filename));
            file.FileName = filename;
            //     file.Description = a.Metadata["Description"].Value<string>();
            //    file.Title = a.Metadata["Description"].Value<string>();
            return file;
        }

        public FileDescription RetrieveThumb(string filename)
        {
            return RetrieveFile(filename);
        }

        public void StoreFile(FileDescription file)
        {
            var longFileName = BuildFileName(file.FileName);
            if (File.Exists(longFileName))
                DeleteFile(file.FileName);
            using (var o = File.Open(
                BuildFileName(file.FileName),
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
            return Path.Combine(_basePath, fileName);
        }

        public string GetPathToBakupFile()
        {
            return _basePath;
        }

        public void RestoreFromBakupFolder(string path)
        {
             var dirWithImeges = Path.Combine(path, ImageFolder);
            foreach (var file in Directory.EnumerateFiles(_basePath))
            {
                File.Delete(file);
            }

            foreach (var file in Directory.GetFiles(dirWithImeges))
                File.Copy(file, Path.Combine(_basePath, Path.GetFileName(file)));
        }
    }
}