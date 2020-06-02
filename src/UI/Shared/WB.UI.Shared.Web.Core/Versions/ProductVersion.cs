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
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(this.assembly.Location);
            if (!fileVersionInfo.ProductVersion.Contains("-"))
            {
                // build is on release branch and has no version suffix
                var version = Version.Parse(fileVersionInfo.FileVersion);

                return FormatVersion(version);
            }
            
            return fileVersionInfo.ProductVersion;
        }

        public Version GetVersion() => new Version(this.ToString().Split(' ')[0]);
        public int GetBuildNumber() => FileVersionInfo.GetVersionInfo(this.assembly.Location).FilePrivatePart;

        public static string FormatVersion(Version version)
        {
            var result = $"{version.Major}.{version.Minor:00}";
            if (version.Build != 0)
            {
                result += $".{version.Build}";
            }

            return result + $" (build {version.MinorRevision})";
        }
    }
}
