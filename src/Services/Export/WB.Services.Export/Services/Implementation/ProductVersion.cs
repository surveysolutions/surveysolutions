using System.Diagnostics;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Services.Implementation
{
    public class ProductVersion : IProductVersion
    {
        public ProductVersion()
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(typeof(ProductVersion).Assembly.Location);

            if (fvi.IsPreReleaseVersion())
            {
                Version = fvi.ProductVersion;
            }
            else
            {
                Version = fvi.ProductVersion;
            }
        }

        private string Version { get; }

        public override string ToString()
        {
            return Version;
        }
    }
}
