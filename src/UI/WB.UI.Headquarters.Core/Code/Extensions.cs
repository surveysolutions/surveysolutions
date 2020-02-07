﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Net.Http.Headers;
using Microsoft.Security.Application;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

            var json = model != null ? JsonConvert.SerializeObject(model, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }) : "null";

            return new HtmlString($@"<script>{script};window.CONFIG.model={json}</script>");
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
