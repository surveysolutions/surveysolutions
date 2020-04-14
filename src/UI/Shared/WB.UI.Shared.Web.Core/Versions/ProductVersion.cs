using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Shared.Web.Versions
{
    public class ProductVersion : IProductVersion
    {
        private readonly Assembly assembly;

        public ProductVersion(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public override string ToString()
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(this.assembly.Location);
            if (!fileVersionInfo.ProductVersion.Contains("-"))
            {
                // build is on release branch and has no version suffix
                var version = Version.Parse(fileVersionInfo.FileVersion);
                var result = $"{version.Major}.{version.Minor:00}";
                if (version.MajorRevision != 0)
                {
                    result += $".{version.MajorRevision}";
                }

                return result + $" (build {version.MinorRevision})";
            }
            
            return fileVersionInfo.ProductVersion;
        }

        public Version GetVersion() => new Version(this.ToString().Split(' ')[0]);
        public int GetBildNumber() => FileVersionInfo.GetVersionInfo(this.assembly.Location).FilePrivatePart;
    }
}
