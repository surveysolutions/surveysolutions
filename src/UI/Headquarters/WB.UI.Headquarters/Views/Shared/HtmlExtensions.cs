using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Settings;

namespace ASP
{
    [Localizable(false)]
    public static partial class HtmlExtensions
    {
        
        

        public static IHtmlString SubstituteQuestionnaireName(this HtmlHelper html,
            string template,
            string questionnaireName)
        {
            if (string.IsNullOrWhiteSpace(template)) return MvcHtmlString.Empty;

            return new MvcHtmlString(template.Replace("%QUESTIONNAIRE%", questionnaireName).Replace("%SURVEYNAME%", questionnaireName));
        }

        public static MvcHtmlString HasErrorClassFor<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression)
        {
            string expressionText = ExpressionHelper.GetExpressionText(expression);

            string htmlFieldPrefix = htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix;

            string fullyQualifiedName;

            if (htmlFieldPrefix.Length > 0)
            {
                fullyQualifiedName = string.Join(".", htmlFieldPrefix, expressionText);
            }
            else
            {
                fullyQualifiedName = expressionText;
            }

            bool isValid = htmlHelper.ViewData.ModelState.IsValidField(fullyQualifiedName);

            if (!isValid)
            {
                return MvcHtmlString.Create("has-error");
            }

            return MvcHtmlString.Empty;
        }

        

        private static readonly JsonSerializerSettings asJsonValueSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static IHtmlString RenderHqConfig(this HtmlHelper helper, object model, string title = null)
        {
            string titleString = title ?? (string) helper.ViewBag.Title?.ToString() ?? null;

            string script = "";

            if (!string.IsNullOrWhiteSpace(titleString))
            {
                script += $"window.CONFIG.title=\"{helper.ToSafeJavascriptMessage(titleString)}\"";
            }

            var json = model != null ? JsonConvert.SerializeObject(model, asJsonValueSettings) : "null";
            
            return new HtmlString($@"<script>{script};window.CONFIG.model={json}</script>");
        }

        public static string UrlScheme(this HttpRequestBase request)
        {
            if (CoreSettings.IsHttpsRequired)
            {
                return "https";
            }

            return request?.Url?.Scheme ?? "http";
        }

        public static string UrlScheme(this HttpRequest request)
        {
            if (CoreSettings.IsHttpsRequired)
            {
                return "https";
            }

            return request.Url.Scheme;
        }
    }
}
