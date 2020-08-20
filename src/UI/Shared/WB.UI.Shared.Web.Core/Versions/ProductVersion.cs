using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Shared.Web.Versions
{
    public class ProductVersion : IProductVersion
    {
        public ProductVersion(Assembly assembly)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (!fileVersionInfo.ProductVersion.Contains("-"))
            {
                // build is on release branch and has no version suffix
                this.version = FormatVersion(Version.Parse(fileVersionInfo.FileVersion));
            }

            this.version =  fileVersionInfo.ProductVersion;
            this.getVersion = Version.TryParse(this.version.Split(' ')[0], out var ver) ? ver : null;
            
            this.buildNumber = fileVersionInfo.FilePrivatePart;
        }

        private readonly string version;
        private readonly int buildNumber;
        private readonly Version getVersion;

        public override string ToString() => version;

        public Version GetVersion() => getVersion;

        public int GetBuildNumber() => buildNumber;

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
