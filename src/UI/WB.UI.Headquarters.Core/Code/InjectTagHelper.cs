using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;

namespace WB.UI.Headquarters.Code
{
    [HtmlTargetElement("locale")]
    public class LocaleTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IUrlHelperFactory urlHelperFactory;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public LocaleTagHelper(IWebHostEnvironment webHostEnvironment, IUrlHelperFactory urlHelperFactory)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.urlHelperFactory = urlHelperFactory;
        }

        public override int Order => 1;

        [HtmlAttributeName("component")]
        public string Component { get; set; }

        [HtmlAttributeName("path")]
        public string Path { get; set; } = "locale/";

        private static readonly Regex ComponentMatcher = new Regex(@"(?<component>[\w\d-]*)\.([\da-f]*)?\.?(min\.)?(json)", RegexOptions.Compiled);

        // TODO: Add memory caching
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var folder = Path.TrimEnd('/') + "/" + Component;
            var files = webHostEnvironment.WebRootFileProvider.GetDirectoryContents(folder);

            Dictionary<string, string> locales = new Dictionary<string, string>();

            foreach (var file in files)
            {
                var match = ComponentMatcher.Match(file.Name);

                if (match.Success == false) continue;

                locales.Add(match.Groups["component"].Value, folder + '/' + file.Name);
            }

            output.TagName = "script";
            var current = System.Globalization.CultureInfo.CurrentUICulture.Name.Split('-')[0];
            if (!locales.ContainsKey(current))
            {
                current = "en";
            }

            output.Content.AppendHtml($@"
window.CONFIG.locale = {{
    locale: '{current}',
    locales: '{JsonConvert.SerializeObject(locales)}'
}};
// JSONP callback to load localization from '@localePath'
window.__setLocaleData__ = function(data) {{ window.CONFIG.locale.data = data; }}
");
            output.PostElement.AppendHtml($"<script src='{locales[current]}'></script>");
            output.PreElement.AppendHtml($"<!-- Locale {Component} -->");
            return Task.CompletedTask;
        }

        private string GetServerPath(string file)
        {
            var contentRootPath = file.Replace(webHostEnvironment.WebRootPath, "~");
            return urlHelperFactory.GetUrlHelper(ViewContext).Content(contentRootPath.Replace("\\", "/"));
        }
    }

    [HtmlTargetElement("inject")]
    public class InjectTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IUrlHelperFactory urlHelperFactory;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public InjectTagHelper(IWebHostEnvironment webHostEnvironment, IUrlHelperFactory urlHelperFactory)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.urlHelperFactory = urlHelperFactory;
        }

        public override int Order => 1;

        [HtmlAttributeName("component")]
        public string Component { get; set; }

        [HtmlAttributeName("async")]
        public bool Async { get; set; }

        public enum Type { css, js }

        [HtmlAttributeName("fallback-to-js")]
        public bool FallbackToJs { get; set; }

        [HtmlAttributeName("type")]
        public Type Extension { get; set; }

        private static readonly Regex ComponentMatcher = new Regex(@"(?<component>[\w\d-]*)\.([\da-f]*)?\.?(min\.)?(css|js)", RegexOptions.Compiled);

        // TODO: Add memory caching
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var extension = Extension;

            InjectTag(output, extension);

            if (output.TagName == "inject" && FallbackToJs)
            {
                InjectTag(output, Type.js);
            }

            output.PreElement.AppendHtml($"<!-- {Component}.{Extension} -->");
            return Task.CompletedTask;
        }

        private void InjectTag(TagHelperOutput output, Type extension)
        {
            foreach (var entry in webHostEnvironment.WebRootFileProvider.GetDirectoryContents("static/" + extension))
            {
                var match = ComponentMatcher.Match(entry.Name);

                if (!match.Success) continue;

                if (match.Groups["component"].Value != Component) continue;

                if (extension == Type.css)
                {
                    output.TagName = "link";
                    output.Attributes.Add("rel", "stylesheet");
                    output.Attributes.Add("href", GetServerPath(entry.PhysicalPath));
                    output.TagMode = TagMode.SelfClosing;
                    if (Async)
                    {
                        output.Attributes.Add(new TagHelperAttribute("async"));
                    }

                    break;
                }

                if (extension == Type.js)
                {
                    output.TagName = "script";
                    output.Attributes.Add("rel", "stylesheet");
                    output.Attributes.Add("src", GetServerPath(entry.PhysicalPath));
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                }

                throw new ArgumentOutOfRangeException();
            }
        }

        private string GetServerPath(string file)
        {
            var contentRootPath = file.Replace(webHostEnvironment.WebRootPath, "~");
            return urlHelperFactory.GetUrlHelper(ViewContext).Content(contentRootPath.Replace("\\", "/"));
        }
    }
}
