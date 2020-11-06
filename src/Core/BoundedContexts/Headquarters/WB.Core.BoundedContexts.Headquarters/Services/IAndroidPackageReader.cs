using System.IO;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IAndroidPackageReader
    {
        AndroidPackageInfo Read(string pathToApkFile);
        AndroidPackageInfo Read(Stream fileStream);
    }
}
