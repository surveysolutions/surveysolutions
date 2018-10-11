using System.Diagnostics;

namespace WB.Services.Export.Services.Implementation
{
    public class ProductVersion : IProductVersion
    {
        public override string ToString() => FileVersionInfo.GetVersionInfo(typeof(ProductVersion).Assembly.Location).ProductVersion;
    }
}
