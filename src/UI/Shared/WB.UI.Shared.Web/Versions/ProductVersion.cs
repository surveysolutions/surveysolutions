using System.Diagnostics;
using System.Reflection;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Shared.Web.Versions
{
    internal class ProductVersion : IProductVersion
    {
        private readonly Assembly assembly;

        public ProductVersion(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public override string ToString() => FileVersionInfo.GetVersionInfo(this.assembly.Location).ProductVersion;
    }
}