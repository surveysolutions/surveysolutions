using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace RavenQuestionnaire.Web.Utils
{
    public static class ViewHelpers
    {
        public static MvcHtmlString ActionLinkWithIcon(this AjaxHelper helper, string linkText,string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes, string icon )
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

    }
}