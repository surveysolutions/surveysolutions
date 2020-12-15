using System;
using System.Diagnostics;
using System.Reflection;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Designer.CommonWeb
{
    public class ProductVersion : IProductVersion
    {
        private readonly Assembly assembly;
        private string? productVersion;
        private int? buildNumber;

        public ProductVersion()
        {
            this.assembly = typeof(Startup).Assembly;
        }

        public override string ToString()
        {
            return productVersion ??= FileVersionInfo.GetVersionInfo(this.assembly.Location).ProductVersion!;
        }

        public Version GetVersion() => new Version(this.ToString().Split(' ')[0]);

        public int GetBuildNumber()
        {
            this.buildNumber ??= FileVersionInfo.GetVersionInfo(this.assembly.Location).FilePrivatePart;
            return buildNumber.Value;
        }
    }
}
