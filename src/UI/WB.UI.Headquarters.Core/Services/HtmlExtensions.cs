using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Services
{
    public static class HtmlExtensions
    {
        public static IHtmlContent  MainMenuItem(this IHtmlHelper html, string actionName, string controllerName, string linkText, MenuItem renderedPage)
        {
            var page = html.ViewBag.ActivePage ?? MenuItem.Logon;
            string isActive = page == renderedPage ? "active" : String.Empty;

            string liStartTag = $"<li class='{isActive}'>{Environment.NewLine}";
            var part2 = html.ActionLink(linkText, actionName, controllerName, new {area = "", id = ""}, new { title = linkText });

            return new HtmlString(liStartTag + part2 + $"{Environment.NewLine}</li>");
        }
    }
}
