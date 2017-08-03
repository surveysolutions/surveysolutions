using System.IO;
using Iteedee.ApkReader;
using System.IO.Compression;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class AndroidPackageReader : IAndroidPackageReader
    {
        public AndroidPackageInfo Read(string pathToApkFile)
        {
            byte[] manifestData = null;
            byte[] resourcesData = null;
            using (ZipArchive zip = new ZipArchive(File.OpenRead(pathToApkFile)))
            {
                var allFound = 0;
                foreach (var zipArchiveEntry in zip.Entries)
                {
                    if (zipArchiveEntry.Name.ToLower() == "androidmanifest.xml")
                    {
                        manifestData = new byte[50 * 1024];
                        using (Stream strm = zipArchiveEntry.Open())
                        {
                            strm.Read(manifestData, 0, manifestData.Length);
                        }
                        allFound++;
                    }
                    if (zipArchiveEntry.Name.ToLower() == "resources.arsc")
                    {
                        using (Stream strm = zipArchiveEntry.Open())
                        {
                            using (BinaryReader s = new BinaryReader(strm))
                            {
                                resourcesData = s.ReadBytes((int)s.BaseStream.Length);

                            }
                        }
                        allFound++;
                    }

                    if(allFound > 1)
                        break;
                }
            }

            var apkInfo = new ApkReader().extractInfo(manifestData, resourcesData);

            return new AndroidPackageInfo
            {
                Version = string.IsNullOrEmpty(apkInfo.versionCode) ? (int?) null : int.Parse(apkInfo.versionCode)
            };
        }
    }
}