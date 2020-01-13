using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Net.Http.Headers;
using Microsoft.Security.Application;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate.SqlCommand;
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

        private static Regex buildVersionRegex = new Regex("build (\\d+)", RegexOptions.Compiled);

        public static Version GetProductVersionFromUserAgent(this HttpRequest request, string productName)
        {
            foreach (var product in request.Headers[HeaderNames.UserAgent])
            {
                if (product.StartsWith(productName, StringComparison.OrdinalIgnoreCase))
                {
                    var parts = product.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        var productVersion = parts[0].Split('/', StringSplitOptions.RemoveEmptyEntries);
                        if (productVersion.Length == 2)
                        {
                            if (Version.TryParse(productVersion[1], out Version version))
                            {
                                return version;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static int? GetBuildNumberFromUserAgent(this HttpRequest request)
        {
            if (request?.Headers?.ContainsKey(HeaderNames.UserAgent) != true) return null;

            foreach (var product in request.Headers[HeaderNames.UserAgent])
            {
                var match = buildVersionRegex.Match(product);
                if (match.Success && match.Groups.Count == 2)
                {
                    var stringBuild = match.Groups[1].Value;
                    if (int.TryParse(stringBuild, out int build))
                    {
                        return build;
                    }
                }
            }

            return null;
        }

        //public static IHtmlContent ToSafeJavascriptMessage(this IHtmlHelper<object> page, string sourceMessage)
        //{
        //    return ToSafeJavascriptMessage(page as HtmlHelper, sourceMessage);
        //}

        public static IHtmlContent ToSafeJavascriptMessage(this IHtmlHelper page, string sourceMessage)
        {
            var html = Encoder.JavaScriptEncode(sourceMessage, false);

            return page.Raw(html);
        }

        public static HtmlString RenderHqConfig(this IHtmlHelper helper, object model, string title = null)
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

        public static T Get<T>(this ISession session, string key)
        {
            var str = session.GetString(key);
            if (str != null)
            {
                return JsonConvert.DeserializeObject<T>(str);
            }

            return default;
        }

        public static void Set<T>(this ISession session, string key, T value)
        {
            if (value != null)
            {
                var str = JsonConvert.SerializeObject(value);
                session.SetString(key, str);
            }
            else
            {
                session.Remove(key);
            }
        }
    }
}
