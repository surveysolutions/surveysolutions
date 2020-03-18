using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace WB.UI.Designer.Extensions
{
    public static class EnvironmentExtensions
    {
        public static string MapPath(this IWebHostEnvironment hostingEnvironment, string path)
        {
            var targetPath = Path.Combine(hostingEnvironment.WebRootPath, path);
            return targetPath;
        }
    }
}
