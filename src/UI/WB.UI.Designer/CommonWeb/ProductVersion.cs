using System;
using System.Diagnostics;
using System.Reflection;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Designer.CommonWeb
{
    public class ProductVersion : IProductVersion
    {
        private Assembly assembly;
        private string productVersion;
        private int? bildNumber;

        public ProductVersion()
        {
            this.assembly = typeof(Startup).Assembly;
        }

        public override string ToString()
        {
            if (productVersion == null)
            {
                productVersion = FileVersionInfo.GetVersionInfo(this.assembly.Location).ProductVersion;
            }

            return productVersion;
        }

        public Version GetVersion() => new Version(this.ToString().Split(' ')[0]);

        public int GetBildNumber()
        {
            if (this.bildNumber == null)
            {
                this.bildNumber = FileVersionInfo.GetVersionInfo(this.assembly.Location).FilePrivatePart;
            }
            return bildNumber.Value;
        }
    }
}
