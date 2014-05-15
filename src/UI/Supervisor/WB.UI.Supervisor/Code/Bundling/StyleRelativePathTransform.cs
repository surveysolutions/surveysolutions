using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;

namespace WB.UI.Supervisor.Code.Bundling
{
    public class StyleImagePathBundle
    : Bundle
    {
        public StyleImagePathBundle(string virtualPath)
            : base(virtualPath)
        {
            base.Transforms.Add(new StyleRelativePathTransform());
            base.Transforms.Add(new CssMinify());
        }

        public StyleImagePathBundle(string virtualPath, string cdnPath)
            : base(virtualPath, cdnPath)
        {
            base.Transforms.Add(new StyleRelativePathTransform());
            base.Transforms.Add(new CssMinify());
        }
    }

    public class StyleRelativePathTransform : IBundleTransform
    {
        public StyleRelativePathTransform()
        {
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            response.Content = String.Empty;

            Regex pattern = new Regex(@"url\s*\(\s*([""']?)([^:)]+)\1\s*\)", RegexOptions.IgnoreCase);
            // open each of the files
            foreach (BundleFile bundleFile in response.Files)
            {
                FileInfo cssFileInfo = new FileInfo(HttpContext.Current.Server.MapPath(bundleFile.IncludedVirtualPath));
                if (!cssFileInfo.Exists) continue;
                // apply the RegEx to the file (to change relative paths)
                string contents = File.ReadAllText(cssFileInfo.FullName);
                MatchCollection matches = pattern.Matches(contents);
                // Ignore the file if no match 
                if (matches.Count > 0)
                {
                    string cssFilePath = cssFileInfo.DirectoryName;
                    string cssVirtualPath = context.HttpContext.RelativeFromAbsolutePath(cssFilePath);
                    foreach (Match match in matches)
                    {
                        // this is a path that is relative to the CSS file
                        string relativeToCSS = match.Groups[2].Value;
                        // combine the relative path to the cssAbsolute
                        string absoluteToUrl = Path.GetFullPath(Path.Combine(cssFilePath, relativeToCSS));

                        // make this server relative
                        string serverRelativeUrl = context.HttpContext.RelativeFromAbsolutePath(absoluteToUrl);

                        string quote = match.Groups[1].Value;
                        string replace = String.Format("url({0}{1}{0})", quote, serverRelativeUrl);
                        contents = contents.Replace(match.Groups[0].Value, replace);
                    }
                }
                // copy the result into the response.
                response.Content = String.Format("{0}\r\n{1}", response.Content, contents);
            }
        }
    }

    public static class PathExtension
    {
        public static string RelativeFromAbsolutePath(this HttpContextBase context, string path)
        {
            var request = context.Request;
            var applicationPath = request.PhysicalApplicationPath;
            var virtualDir = request.ApplicationPath;
            virtualDir = virtualDir == "/" ? virtualDir : (virtualDir + "/");
            return path.Replace(applicationPath, virtualDir).Replace(@"\", "/");
        }
    }
}