using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace WB.UI.Shared.Web.Extensions
{
    [HtmlTargetElement("locale")]
    public class LocaleTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IMemoryCache memoryCache;

        public LocaleTagHelper(IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.memoryCache = memoryCache;
        }

        public override int Order => 1;

        [HtmlAttributeName("component")]
        public string Component { get; set; }

        [HtmlAttributeName("path")]
        public string Path { get; set; } = "locale/";

        private static readonly Regex ComponentMatcher = new Regex(@"(?<component>[\w\d-]*)\.([\da-f]*)?\.?(min\.)?(json)", RegexOptions.Compiled);

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var folder = Path.TrimEnd('/') + "/" + Component;

            var key = $"tag::{folder}";
            var locales = this.memoryCache.GetOrCreate(key, entry =>
            {
                var files = webHostEnvironment.WebRootFileProvider.GetDirectoryContents(folder);

                Dictionary<string, string> localeFiles = new Dictionary<string, string>();

                foreach (var file in files.OrderByDescending(f => f.LastModified))
                {
                    var match = ComponentMatcher.Match(file.Name);

                    if (match.Success == false) continue;

                    if (localeFiles.ContainsKey(match.Groups["component"].Value)) continue;

                    localeFiles.Add(match.Groups["component"].Value, '/' + folder + '/' + file.Name);
                }

                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                entry.AddExpirationToken(webHostEnvironment.WebRootFileProvider.Watch(folder + "/" + "*.json"));
                return localeFiles;
            });

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
            if (locales.ContainsKey(current))
            {
                output.PostElement.AppendHtml($"<script src='{locales[current]}'></script>");
            }

            output.PreElement.AppendHtml($"<!-- Locale {Component} -->");
            return Task.CompletedTask;
        }
    }
}
