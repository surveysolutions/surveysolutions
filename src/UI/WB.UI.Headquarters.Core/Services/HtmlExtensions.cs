using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Services
{
    public static class HtmlExtensions
    {
        private static readonly JsonSerializerSettings asJsonValueSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static IHtmlContent  MainMenuItem(this IHtmlHelper html, string actionName, string controllerName, string linkText, MenuItem renderedPage)
        {
            var page = html.ViewBag.ActivePage ?? MenuItem.Logon;
            string isActive = page == renderedPage ? "active" : String.Empty;

            string liStartTag = $"<li class='{isActive}'>{Environment.NewLine}";
            var part2 = html.ActionLink(linkText, actionName, controllerName, new {area = "", id = ""}, new { title = linkText });

            return new HtmlString(liStartTag + part2 + $"{Environment.NewLine}</li>");
        }

        
        public static IHtmlContent AsJsonValue(this object obj)
        {
            return new HtmlString(JsonConvert.SerializeObject(obj, asJsonValueSettings));
        }
    }
}
