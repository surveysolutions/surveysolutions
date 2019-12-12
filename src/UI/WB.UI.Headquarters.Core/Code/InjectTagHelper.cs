using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WB.UI.Headquarters.Code
{
    public class InjectTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IUrlHelperFactory urlHelperFactory;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public InjectTagHelper(IWebHostEnvironment webHostEnvironment, IUrlHelperFactory  urlHelperFactory)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.urlHelperFactory = urlHelperFactory;
        }
        
        public override int Order => 1;

        [HtmlAttributeName("component")]
        public string Component { get; set; }

        [HtmlAttributeName("async")]
        public bool Async { get; set; }

        public enum Type { css, js}

        [HtmlAttributeName("type")]
        public Type Extension { get; set; }

        private static readonly Regex ComponentMatcher = new Regex(@"(?<component>[\w\d-]*)\.([\da-f]*)\.(min\.)?(css|js)", RegexOptions.Compiled);

        // TODO: Add memory caching
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            foreach (var entry in webHostEnvironment.WebRootFileProvider.GetDirectoryContents("static/" + Extension))
            {
                var match = ComponentMatcher.Match(entry.Name);

                if (!match.Success) continue;

                if (match.Groups["component"].Value != Component) continue;

                switch (Extension)
                {
                    case Type.css:
                        output.TagName = "link";
                        output.Attributes.Add("rel", "stylesheet");
                        output.Attributes.Add("href", GetServerPath(entry.PhysicalPath));
                        output.TagMode = TagMode.SelfClosing;
                        if (Async)
                        {
                            output.Attributes.Add(new TagHelperAttribute("async"));
                        }

                        break;
                    case Type.js:
                        output.TagName = "script";
                        output.Attributes.Add("rel", "stylesheet");
                        output.Attributes.Add("src", GetServerPath(entry.PhysicalPath));
                        output.TagMode = TagMode.StartTagAndEndTag;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            }

            return Task.CompletedTask;
        }

        private string GetServerPath(string file)
        {
            var contentRootPath = file.Replace(webHostEnvironment.WebRootPath, "~");
            return urlHelperFactory.GetUrlHelper(ViewContext).Content(contentRootPath.Replace("\\", "/"));
        }
    }
}
