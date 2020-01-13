﻿using System;
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
    }
}
