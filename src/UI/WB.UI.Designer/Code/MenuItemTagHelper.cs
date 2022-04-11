using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WB.UI.Designer.Code
{
    public class MenuItemTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory url;

        public MenuItemTagHelper(IUrlHelperFactory url)
        {
            this.url = url;
        }

        [HtmlAttributeName("asp-area")]
        public string? Area { get; set; }
        
        [HtmlAttributeName("asp-page")]
        public string? Page { get; set; }

        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; } = String.Empty;

        [HtmlAttributeName("asp-action")]
        public string Action { get; set; } = String.Empty;
        
        [HtmlAttributeName("asp-title")]
        public string Title { get; set; } = String.Empty;

        [HtmlAttributeName("asp-if")]
        public bool? Condition { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (Condition.HasValue && Condition.Value == false)
            {
                output.SuppressOutput();
                return;
            }

            var childContent = output.Content.IsModified 
                ? output.Content.GetContent(NullHtmlEncoder.Default) 
                : (await output.GetChildContentAsync(NullHtmlEncoder.Default)).GetContent(NullHtmlEncoder.Default);

            output.TagName = "li";
            output.Attributes.Add(new TagHelperAttribute("role", "presentation"));
            HandleActiveFlag(output);
            
            var anchor = new TagBuilder("a");
            string? href;
            if (IsPageTag)
            {
                href = ViewContext.HttpContext.Request.PathBase + (string.IsNullOrWhiteSpace(Area) ? Page : "/" + Area + Page);
            }
            else
            {
                href = url.GetUrlHelper(ViewContext)
                    .Action(Action, Controller, new {area = Area, id = (Guid?) null});
            }

            if (href == null) 
                throw new InvalidOperationException($"Invalid href for IsPageTag: {IsPageTag}, Area: {Area}, Page: {Page}, Action: {Action}, Controller: {Controller}");
            anchor.Attributes.Add("href", href);

            anchor.InnerHtml.Append(childContent);
            output.Content.SetHtmlContent(anchor);
        }

        private bool IsPageTag => !string.IsNullOrWhiteSpace(Page) && string.IsNullOrWhiteSpace(Action);

        private void HandleActiveFlag(TagHelperOutput output)
        {
            if (IsPageTag && IsActivePage() || !IsPageTag && IsActiveRoute())
            {
                output.Attributes.SetAttribute("class", "active");
            }
        }

        private bool IsActiveRoute()
        {
            var currentAction = ViewContext.CurrentAction();
            var currentController = ViewContext.CurrentController();
            var currentArea = ViewContext.CurrentArea();

            if (!string.IsNullOrWhiteSpace(Action) && !Action.Equals(currentAction))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Controller) && !Controller.Equals(currentController))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Area))
            {
                if (!Area.Equals(currentArea, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            foreach (KeyValuePair<string, string> routeValue in RouteValues)
            {
                if (!ViewContext.RouteData.Values.ContainsKey(routeValue.Key) ||
                    ViewContext.RouteData.Values[routeValue.Key]!.ToString() != routeValue.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsActivePage()
        {
            var currentPage = ViewContext.CurrentPage();
            var currentArea = ViewContext.CurrentArea();
            
            if (!string.IsNullOrWhiteSpace(Page) && !Page.Equals(currentPage, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Area))
            {
                if (!Area.Equals(currentArea, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            
            foreach (KeyValuePair<string, string> routeValue in RouteValues)
            {
                if (!ViewContext.RouteData.Values.ContainsKey(routeValue.Key) ||
                    ViewContext.RouteData.Values[routeValue.Key]!.ToString() != routeValue.Value)
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Gets or sets the <see cref="T:Microsoft.AspNetCore.Mvc.Rendering.ViewContext" /> for the current request.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; } = new ViewContext();

        /// <summary>Additional parameters for the route.</summary>
        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues { get; set; } = (IDictionary<string, string>)new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
    }
}
