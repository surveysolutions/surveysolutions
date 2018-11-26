using System;
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

        public override string ToString() => FileVersionInfo.GetVersionInfo(this.assembly.Location).ProductVersion;
        public Version GetVersion() => new Version(this.ToString().Split(' ')[0]);
        public int GetBildNumber() => System.Diagnostics.FileVersionInfo.GetVersionInfo(this.assembly.Location).FilePrivatePart;
    }
}
