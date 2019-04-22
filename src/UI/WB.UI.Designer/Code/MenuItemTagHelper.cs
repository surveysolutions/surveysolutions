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
        public string Area { get; set; }
        
        [HtmlAttributeName("asp-page")]
        public string Page { get; set; }

        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; }

        [HtmlAttributeName("asp-action")]
        public string Action { get; set; }
        
        [HtmlAttributeName("asp-title")]
        public string Title { get; set; }

        [HtmlAttributeName("asp-if")]
        public bool? Condition { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (Condition.HasValue && Condition.Value == false)
            {
                output.SuppressOutput();
                return;
            }

            var childContent = output.Content.IsModified ? output.Content.GetContent() :
                (await output.GetChildContentAsync()).GetContent();

            output.TagName = "li";
            output.Attributes.Add(new TagHelperAttribute("role", "presentation"));
            HandleActiveFlag(output);
            
            var anchor = new TagBuilder("a");
            string href;
            if (IsPageTag)
            {
                href = string.IsNullOrWhiteSpace(Area) ? Page : "/" + Area + Page;
            }
            else
            {
                href = url.GetUrlHelper(ViewContext).Action(Action, Controller, string.IsNullOrWhiteSpace(Area) ? null : new {area = Area});
            }
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
            string currentAction = ViewContext.CurrentAction();
            string currentController = ViewContext.CurrentController();
            string currentArea = ViewContext.CurrentArea();

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
                    ViewContext.RouteData.Values[routeValue.Key].ToString() != routeValue.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsActivePage()
        {
            string currentPage = ViewContext.CurrentPage();
            string currentArea = ViewContext.CurrentArea();
            
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
                    ViewContext.RouteData.Values[routeValue.Key].ToString() != routeValue.Value)
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
        public ViewContext ViewContext { get; set; }

        private IDictionary<string, string> _routeValues;

        /// <summary>Additional parameters for the route.</summary>
        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues
        {
            get
            {
                if (this._routeValues == null)
                    this._routeValues = (IDictionary<string, string>)new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
                return this._routeValues;
            }
            set => this._routeValues = value;
        }
    }

    //[HtmlTargetElement(Attributes = "is-active-page")]
    //public class ActivePageTagHelper : TagHelper
    //{
    //    private IDictionary<string, string> _routeValues;

    //    /// <summary>The name of the action method.</summary>
    //    /// <remarks>Must be <c>null</c> if <see cref="P:Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper.Route" /> is non-<c>null</c>.</remarks>
    //    [HtmlAttributeName("asp-area")]
    //    public string Area { get; set; }

    //    /// <summary>The name of the controller.</summary>
    //    /// <remarks>Must be <c>null</c> if <see cref="P:Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper.Route" /> is non-<c>null</c>.</remarks>
    //    [HtmlAttributeName("asp-page")]
    //    public string Page { get; set; }

    //    /// <summary>Additional parameters for the route.</summary>
    //    [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
    //    public IDictionary<string, string> RouteValues
    //    {
    //        get
    //        {
    //            if (this._routeValues == null)
    //                this._routeValues = (IDictionary<string, string>)new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
    //            return this._routeValues;
    //        }
    //        set => this._routeValues = value;
    //    }

    //    /// <summary>
    //    /// Gets or sets the <see cref="T:Microsoft.AspNetCore.Mvc.Rendering.ViewContext" /> for the current request.
    //    /// </summary>
    //    [HtmlAttributeNotBound]
    //    [ViewContext]
    //    public ViewContext ViewContext { get; set; }

    //    public override void Process(TagHelperContext context, TagHelperOutput output)
    //    {
    //        base.Process(context, output);

    //        if (ShouldBeActive())
    //        {
    //            MakeActive(output);
    //        }

    //        output.Attributes.RemoveAll("is-active-route");
    //    }

    //    private bool ShouldBeActive()
    //    {
    //        string currentPage = ViewContext.CurrentPage();
    //        string currentArea = ViewContext.CurrentArea();

    //        if (!string.IsNullOrWhiteSpace(Page) && !Page.Equals(currentPage, StringComparison.OrdinalIgnoreCase))
    //        {
    //            return false;
    //        }

    //        if (!string.IsNullOrWhiteSpace(Area))
    //        {
    //            if (!Area.Equals(currentArea, StringComparison.OrdinalIgnoreCase))
    //            {
    //                return false;
    //            }
    //        }

    //        foreach (KeyValuePair<string, string> routeValue in RouteValues)
    //        {
    //            if (!ViewContext.RouteData.Values.ContainsKey(routeValue.Key) ||
    //                ViewContext.RouteData.Values[routeValue.Key].ToString() != routeValue.Value)
    //            {
    //                return false;
    //            }
    //        }

    //        return true;
    //    }

    //    private void MakeActive(TagHelperOutput output)
    //    {
    //        var classAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
    //        if (classAttr == null)
    //        {
    //            classAttr = new TagHelperAttribute("class", "active");
    //            output.Attributes.Add(classAttr);
    //        }
    //        else if (classAttr.Value == null || classAttr.Value.ToString().IndexOf("active") < 0)
    //        {
    //            output.Attributes.SetAttribute("class", classAttr.Value == null
    //                ? "active"
    //                : classAttr.Value.ToString() + " active");
    //        }
    //    }
    //}
}
