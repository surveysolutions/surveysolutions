using System.Diagnostics;

namespace WB.Services.Export.Services.Implementation
{
    public class ProductVersion : IProductVersion
    {
        public ProductVersion()
        {
            var fvi = FileVersionInfo.GetVersionInfo(typeof(ProductVersion).Assembly.Location);
            version = fvi.ProductVersion;
        }

        private readonly string? version;

        public override string? ToString()
        {
            return version;
        }
    }
}
