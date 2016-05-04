using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Iteedee.ApkReader;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class AndroidPackageReader : IAndroidPackageReader
    {
        public AndroidPackageInfo Read(string pathToApkFile)
        {
            byte[] manifestData = null;
            byte[] resourcesData = null;
            using (ZipInputStream zip = new ZipInputStream(File.OpenRead(pathToApkFile)))
            {
                using (var filestream = new FileStream(pathToApkFile, FileMode.Open, FileAccess.Read))
                {
                    ZipFile zipfile = new ZipFile(filestream);
                    ZipEntry item;
                    while ((item = zip.GetNextEntry()) != null)
                    {
                        if (item.Name.ToLower() == "androidmanifest.xml")
                        {
                            manifestData = new byte[50 * 1024];
                            using (Stream strm = zipfile.GetInputStream(item))
                            {
                                strm.Read(manifestData, 0, manifestData.Length);
                            }

                        }
                        if (item.Name.ToLower() == "resources.arsc")
                        {
                            using (Stream strm = zipfile.GetInputStream(item))
                            {
                                using (BinaryReader s = new BinaryReader(strm))
                                {
                                    resourcesData = s.ReadBytes((int)s.BaseStream.Length);

                                }
                            }
                        }
                    }
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