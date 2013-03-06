using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Main.Core.Documents;
using Main.Core.Services;

namespace AndroidApp.Core.Model.FileStorage
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
            var file = new FileDescription();

            file.Content = File.OpenRead(BuildFileName(filename));
            file.PublicKey = filename;
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
            var longFileName = BuildFileName(file.PublicKey);
            if (File.Exists(longFileName))
                DeleteFile(file.PublicKey);
            using (var o = File.Open(
                BuildFileName(file.PublicKey),
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