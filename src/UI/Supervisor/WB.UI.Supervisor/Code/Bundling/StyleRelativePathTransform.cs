using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;

namespace WB.UI.Supervisor.Code.Bundling
{
    public class StyleRelativePathTransform : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            response.Content = String.Empty;

            var pattern = new Regex(@"url\s*\(\s*([""']?)([^:)]+)\1\s*\)", RegexOptions.IgnoreCase);

            foreach (BundleFile bundleFile in response.Files)
            {
                var cssFileInfo = new FileInfo(HttpContext.Current.Server.MapPath(bundleFile.IncludedVirtualPath));
                if (!cssFileInfo.Exists) continue;
                var contents = File.ReadAllText(cssFileInfo.FullName);
                var matches = pattern.Matches(contents);
                if (matches.Count > 0)
                {
                    var cssFilePath = cssFileInfo.DirectoryName;
                    foreach (Match match in matches)
                    {
                        var relativeToCss = match.Groups[2].Value;
                        var absoluteToUrl = Path.GetFullPath(Path.Combine(cssFilePath, relativeToCss));

                        var serverRelativeUrl = context.HttpContext.RelativeFromAbsolutePath(absoluteToUrl);

                        var quote = match.Groups[1].Value;
                        var replace = String.Format("url({0}{1}{0})", quote, serverRelativeUrl);
                        contents = contents.Replace(match.Groups[0].Value, replace);
                    }
                }
                response.Content = String.Format("{0}\r\n{1}", response.Content, contents);
            }
        }
    }
}