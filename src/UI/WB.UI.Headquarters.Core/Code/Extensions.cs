using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Net.Http.Headers;
using Microsoft.Security.Application;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Code
{
    public static class Extensions
    {
        public static bool RequestHasMatchingFileHash(this HttpRequest request, byte[] hash)
        {
            var expectedHash = $@"""{Convert.ToBase64String(hash)}""";

            if (request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
            {
                var nonMatchHeader = request.Headers[HeaderNames.IfNoneMatch].ToString();
                var header = EntityTagHeaderValue.Parse(nonMatchHeader);

                return expectedHash.Equals(header.Tag.ToString(), StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public static Version GetProductVersionFromUserAgent(this HttpRequest request, string productName)
        {
            //foreach (var product in request.Headers[HeaderNames.UserAgent])
            //{
            //    if ((product.Product?.Name.StartsWith(productName, StringComparison.OrdinalIgnoreCase) ?? false)
            //        && Version.TryParse(product.Product.Version, out Version version))
            //    {
            //        return version;
            //    }
            //}

            return null;
        }

        //public static IHtmlContent ToSafeJavascriptMessage(this IHtmlHelper<object> page, string sourceMessage)
        //{
        //    return ToSafeJavascriptMessage(page as HtmlHelper, sourceMessage);
        //}

        public static IHtmlContent ToSafeJavascriptMessage<T>(this IHtmlHelper<T> page, string sourceMessage)
        {
            var html = Encoder.JavaScriptEncode(sourceMessage, false);

            return page.Raw(html);
        }

        public static HtmlString RenderHqConfig<T>(this IHtmlHelper<T> helper, object model, string title = null)
        {
            string titleString = title ?? (string)helper.ViewBag.Title?.ToString() ?? null;

            string script = "";

            if (!string.IsNullOrWhiteSpace(titleString))
            {
                script += $"window.CONFIG.title=\"{helper.ToSafeJavascriptMessage(titleString)}\"";
            }

            var json = model != null ? JsonConvert.SerializeObject(model, asJsonValueSettings) : "null";

            return new HtmlString($@"<script>{script};window.CONFIG.model={json}</script>");
        }

        private static readonly JsonSerializerSettings asJsonValueSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static string QuestionnaireName(this IHtmlHelper html, string name, long version)
        {
            return string.Format(Pages.QuestionnaireNameFormat, name, version);
        }

        public static string QuestionnaireNameVerstionFirst(this IHtmlHelper html, string name, long version)
        {
            return string.Format(Pages.QuestionnaireNameVersionFirst, name, version);
        }
    }
}
