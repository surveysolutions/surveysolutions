using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace RavenQuestionnaire.Web.Utils
{
    public static class ViewHelpers
    {
        public static MvcHtmlString ActionLinkWithIcon(this AjaxHelper helper, string linkText, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes, string icon)
        {
            var builder = new TagBuilder("i");
            builder.MergeAttribute("class", icon);
            var link = helper.ActionLink("[replaceme]" + linkText, actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);
            return MvcHtmlString.Create(link.ToHtmlString().Replace("[replaceme]", builder.ToString(TagRenderMode.Normal)));
        }


        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
                                                             Expression<Func<TModel, TValue>> expression,
                                                             object htmlAttributes)
        {
            return LabelFor(html, expression, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
                                                             Expression<Func<TModel, TValue>> expression,
                                                             IDictionary<string, object> htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            string labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (String.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            TagBuilder tag = new TagBuilder("label");
            tag.MergeAttributes(htmlAttributes);
            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));
            tag.SetInnerText(labelText);
            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString BootstrapValidationSummary(this HtmlHelper helper, string validationMessage = "")
        {
            string retVal = "";
            if (helper.ViewData.ModelState.IsValid)
                return MvcHtmlString.Create(string.Empty);
            retVal += "<div class='alert alert-error'><a data-dismiss='alert' class='close'>&times;</a><span>";
            if (!String.IsNullOrEmpty(validationMessage))
                retVal += validationMessage;
            retVal += "</span>";
            retVal += "<ul>";
            retVal = helper.ViewData.ModelState.Keys
                .SelectMany(key => helper.ViewData.ModelState[key].Errors)
                .Aggregate(retVal, (current, err) => current + ("<li>" + err.ErrorMessage + "</li>"));
            retVal += "</ul></div>";
            return MvcHtmlString.Create(retVal);
        }

        public static MvcHtmlString BootstrapValidationMessage(this HtmlHelper htmlHelper, string modelName)
        {
            if (modelName == null)
            {
                throw new ArgumentNullException("modelName");
            }

            if (!htmlHelper.ViewData.ModelState.ContainsKey(modelName))
            {
                return null;
            }

            ModelState modelState = htmlHelper.ViewData.ModelState[modelName];
            ModelErrorCollection modelErrors = (modelState == null) ? null : modelState.Errors;
            ModelError modelError = ((modelErrors == null) || (modelErrors.Count == 0)) ? null : modelErrors[0];

            if (modelError == null)
            {
                return null;
            }


            string retVal = "";
            if (htmlHelper.ViewData.ModelState.IsValid)
                return null;

            var message = GetUserErrorMessageOrDefault(htmlHelper.ViewContext.HttpContext, modelError, modelState);
            Guid id;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(message));
                id = new Guid(hash);
            }

            retVal += "<div  close-marker=\"" + id.ToString() + "\" class='alert alert-error'><a  class='close'>&times;</a><span>";
            if (!String.IsNullOrEmpty(message))
                retVal += message;
            retVal += "</span>";
            retVal += "</div>";
            return MvcHtmlString.Create(retVal);
        }

        private static string GetUserErrorMessageOrDefault(HttpContextBase httpContext, ModelError error, ModelState modelState)
        {
            if (!String.IsNullOrEmpty(error.ErrorMessage))
            {
                return error.ErrorMessage;
            }
            if (modelState == null)
            {
                return null;
            }

            string attemptedValue = (modelState.Value != null) ? modelState.Value.AttemptedValue : null;
            return String.Format(CultureInfo.CurrentCulture, "Invalid property", attemptedValue);
        }
    }
}