using System;
using System.Diagnostics;
using System.Linq;

namespace WB.Services.Export
{
    public static class FileVersionInfoExtension
    {
        public static bool IsPreReleaseVersion(this FileVersionInfo fvi)
        {
            // we consider as pre-released any version that contain letters in product version, i.e.
            // 18.12.0-dev.322 is considered pre-released version
            // 18.12.0.322 - is release version
            return fvi.ProductVersion != null && fvi.ProductVersion.Any(char.IsLetter);
        }
    }
}
