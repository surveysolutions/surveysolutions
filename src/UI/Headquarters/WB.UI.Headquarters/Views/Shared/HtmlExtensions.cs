using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace ASP
{
    public static partial class HtmlExtensions
    {
        public static IHtmlString  MainMenuItem(this HtmlHelper html, string actionName, string controllerName, string linkText, MenuItem renderedPage)
        {
            var page = html.ViewBag.ActivePage ?? MenuItem.Logon;
            string isActive = page == renderedPage ? "active" : String.Empty;

            string liStartTag = $"<li class='{isActive}'>\r\n";
            MvcHtmlString part2 = html.ActionLink(linkText, actionName, controllerName, new {area = ""}, new object());

            return new MvcHtmlString(liStartTag + part2 + "</li>");
        }
    }
}