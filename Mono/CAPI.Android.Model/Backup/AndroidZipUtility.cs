using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace CAPI.Android.Core.Model.Backup
{
    public class AndroidZipUtility
    {
        public static void ZipDirectory(string directory, string zip)
        {
            if (File.Exists(zip))
                throw new InvalidOperationException("zip file exists");

            using (
                var zipFile = new Ionic.Zip.ZipFile()
                    {
                        ParallelDeflateThreshold = -1;
                        AlternateEncoding = System.Text.Encoding.UTF8,
                        AlternateEncodingUsage = ZipOption.Always
                    })
            {
                zipFile.AddDirectory(directory, Path.GetFileName(directory));
                zipFile.Save(zip);
            }
        }

        public static void Unzip(string zip, string extractTo)
        {
            using (ZipFile decompress = ZipFile.Read(zip))
            {
                foreach (ZipEntry e in decompress)
                {
                    e.Extract(extractTo, ExtractExistingFileAction.OverwriteSilently);
                }
            }
            
        }
    }
}