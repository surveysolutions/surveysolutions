using System.IO;
using Main.Core.Documents;
using Main.Core.Services;

namespace CAPI.Android.Core.Model.FileStorage
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string basePath=System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

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
            return Path.Combine(basePath, fileName);
        }
    }
}