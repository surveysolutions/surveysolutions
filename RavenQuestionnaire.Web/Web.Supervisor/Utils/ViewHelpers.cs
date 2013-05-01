// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewHelpers.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the ViewHelpers type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Utils
{
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
    using System.Web.Routing;

    /// <summary>
    /// Responsible for creation additional properties of form elements
    /// </summary>
    public static class ViewHelpers
    {
        public static SelectList ToSelectList<TEnum>(this TEnum enumObj)
        {
            var values = from TEnum e in Enum.GetValues(typeof(TEnum))
                         select new { Id = e, Name = e.ToString() };

            return new SelectList(values, "Id", "Name", enumObj);
        }

        /// <summary>
        /// Creation actionLink with image
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="controllerName">
        /// The controller name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <param name="icon">
        /// The icon.
        /// </param>
        /// <returns>
        /// Return new mvc html element
        /// </returns>
        public static MvcHtmlString ActionLinkWithIcon(this AjaxHelper helper, string linkText, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes, string icon)
        {
            var builder = new TagBuilder("i");
            builder.MergeAttribute("class", icon);
            var link = helper.ActionLink("[replaceme]" + linkText, actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);
            return
                MvcHtmlString.Create(link.ToHtmlString().Replace("[replaceme]", builder.ToString(TagRenderMode.Normal)));
        }

        /// <summary>
        /// Create new properties for LabelFor
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <typeparam name="TModel">
        /// The parameter  TModel
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The parameter TValue
        /// </typeparam>
        /// <returns>
        ///  Return LabelFor with additional properties
        /// </returns>
        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return LabelFor(html, expression, new RouteValueDictionary(htmlAttributes));
        }

        /// <summary>
        /// Added additional properties for LabelFor
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <typeparam name="TModel">
        /// The parameter TModel
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The parameter TValue
        /// </typeparam>
        /// <returns>
        /// Return LabelFor with additional properties
        /// </returns>
        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            var labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (String.IsNullOrEmpty(labelText))
                return MvcHtmlString.Empty;
            var tag = new TagBuilder("label");
            tag.MergeAttributes(htmlAttributes);
            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));
            tag.SetInnerText(labelText);
            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }

        /// <summary>
        /// Added new validation parameters
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="validationMessage">
        /// The validation message.
        /// </param>
        /// <returns>
        /// Return new mvc html element
        /// </returns>
        public static MvcHtmlString BootstrapValidationSummary(this HtmlHelper helper, string validationMessage = "")
        {
            string retVal = string.Empty;
            if (helper.ViewData.ModelState.IsValid)
                return MvcHtmlString.Create(string.Empty);
            retVal += "<div class='alert alert-error'><a data-dismiss='alert' class='close'>&times;</a><span>";
            if (!string.IsNullOrEmpty(validationMessage))
                retVal += validationMessage;
            retVal += "</span>";
            retVal += "<ul>";
            retVal = helper.ViewData.ModelState.Keys
                .SelectMany(key => helper.ViewData.ModelState[key].Errors)
                .Aggregate(retVal, (current, err) => current + ("<li>" + err.ErrorMessage + "</li>"));
            retVal += "</ul></div>";
            return MvcHtmlString.Create(retVal);
        }

        /// <summary>
        /// Responsible for painting new validation properties
        /// </summary>
        /// <param name="htmlHelper">
        /// The html helper.
        /// </param>
        /// <param name="modelName">
        /// The model name.
        /// </param>
        /// <returns>
        /// Return creation html element
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// added new null exception
        /// </exception>
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
                return null;
            string retVal = string.Empty;
            if (htmlHelper.ViewData.ModelState.IsValid)
                return null;
            string message = GetUserErrorMessageOrDefault(htmlHelper.ViewContext.HttpContext, modelError, modelState);
            Guid id;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(message));
                id = new Guid(hash);
            }
            retVal += "<div  close-marker=\"" + id.ToString() +
                      "\" class='alert alert-error'><a  class='close'>&times;</a><span>";
            if (!String.IsNullOrEmpty(message))
                retVal += message;
            retVal += "</span>";
            retVal += "</div>";
            return MvcHtmlString.Create(retVal);
        }

        /// <summary>
        /// Responsible for creation error message
        /// </summary>
        /// <param name="httpContext">
        /// The http context.
        /// </param>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="modelState">
        /// The model state.
        /// </param>
        /// <returns>
        /// Return new error message
        /// </returns>
        private static string GetUserErrorMessageOrDefault(HttpContextBase httpContext, ModelError error, ModelState modelState)
        {
            if (!string.IsNullOrEmpty(error.ErrorMessage)) 
                return error.ErrorMessage;
            if (modelState == null)
                return null;
            var attemptedValue = (modelState.Value != null) ? modelState.Value.AttemptedValue : null;
            return string.Format(CultureInfo.CurrentCulture, "Invalid property", attemptedValue);
        }
    }
}