using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WB.UI.WebTester.Infrastructure
{
    public class InjectTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public InjectTagHelper(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        private readonly string _style =
            @"<link rel=""stylesheet"" href=""/css/address.css"" />";

        public override int Order => 1;

        [HtmlAttributeName("component")]
        public string Component { get; set; }

        public enum Type { css, js}

        [HtmlAttributeName("type")]
        public Type Extension { get; set; }

        private static readonly Regex ComponentMatcher = new Regex(@"(?<component>[\w\d-]*)(-[\da-f])?(\.min)?\.(css|js)", RegexOptions.Compiled);

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            foreach (var dir in webHostEnvironment.WebRootFileProvider.GetDirectoryContents(""))
            {

            }
            
            //if (string.Equals(context.TagName, "head",
            //    StringComparison.OrdinalIgnoreCase))
            //{
            //    output.PostContent.AppendHtml(_style);
            //}

            return Task.CompletedTask;
        }
    }
}
