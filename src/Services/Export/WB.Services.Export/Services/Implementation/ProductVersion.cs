using System.Diagnostics;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Services.Implementation
{
    public class ProductVersion : IProductVersion
    {
        public ProductVersion()
        {
            var fvi = FileVersionInfo.GetVersionInfo(typeof(ProductVersion).Assembly.Location);

            if (fvi.IsPreReleaseVersion())
            {
                Version = fvi.ProductVersion;
            }
            else
            {
                // do not include trailing zero and build number for release
                Version = $"{fvi.FileMajorPart}.{fvi.FileMinorPart}" + (fvi.FileBuildPart == 0 ? "" : $".{fvi.FileBuildPart}");
            }
        }

        private string Version { get; }

        public override string ToString()
        {
            return Version;
        }
    }
}
