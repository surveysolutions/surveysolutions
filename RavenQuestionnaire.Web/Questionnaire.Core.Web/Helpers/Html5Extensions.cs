namespace Questionnaire.Core.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;

    /// <summary>
    /// The html 5 extensions.
    /// </summary>
    public static class Html5Extensions
    {
        #region Constants

        /// <summary>
        /// The form on click value.
        /// </summary>
        private const string FormOnClickValue = "Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));";

        /// <summary>
        /// The form on submit format.
        /// </summary>
        private const string FormOnSubmitFormat =
            "Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), {0});";

        /// <summary>
        /// The link on click format.
        /// </summary>
        private const string LinkOnClickFormat =
            "Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), {0});";

        /// <summary>
        /// The _globalization script.
        /// </summary>
        private const string _globalizationScript = @"<script type=""text/javascript"" src=""{0}""></script>";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, string linkText, string actionName, AjaxOptions ajaxOptions)
        {
            return ActionLinkHtml5(ajaxHelper, linkText, actionName, (string)null /* controllerName */, ajaxOptions);
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, string linkText, string actionName, object routeValues, AjaxOptions ajaxOptions)
        {
            return ActionLinkHtml5(
                ajaxHelper, linkText, actionName, null /* controllerName */, routeValues, ajaxOptions);
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="actionName">
        /// The action name.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string actionName, 
            object routeValues, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            return ActionLinkHtml5(
                ajaxHelper, linkText, actionName, null /* controllerName */, routeValues, ajaxOptions, htmlAttributes);
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string actionName, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions)
        {
            return ActionLinkHtml5(
                ajaxHelper, linkText, actionName, null /* controllerName */, routeValues, ajaxOptions);
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="actionName">
        /// The action name.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string actionName, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            return ActionLinkHtml5(
                ajaxHelper, linkText, actionName, null /* controllerName */, routeValues, ajaxOptions, htmlAttributes);
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string actionName, 
            string controllerName, 
            AjaxOptions ajaxOptions)
        {
            return ActionLinkHtml5(
                ajaxHelper, linkText, actionName, controllerName, null /* values */, ajaxOptions, null
                
                /* htmlAttributes */);
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string actionName, 
            string controllerName, 
            object routeValues, 
            AjaxOptions ajaxOptions)
        {
            return ActionLinkHtml5(
                ajaxHelper, linkText, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string actionName, 
            string controllerName, 
            object routeValues, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            var newValues = new RouteValueDictionary(routeValues);
            RouteValueDictionary newAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return ActionLinkHtml5(
                ajaxHelper, linkText, actionName, controllerName, newValues, ajaxOptions, newAttributes);
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string actionName, 
            string controllerName, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions)
        {
            return ActionLinkHtml5(
                ajaxHelper, linkText, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string actionName, 
            string controllerName, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            if (string.IsNullOrEmpty(linkText))
            {
                throw new ArgumentException("linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(
                null, 
                actionName, 
                controllerName, 
                routeValues, 
                ajaxHelper.RouteCollection, 
                ajaxHelper.ViewContext.RequestContext, 
                true /* includeImplicitMvcValues */);

            return
                MvcHtmlString.Create(
                    GenerateLink(ajaxHelper, linkText, targetUrl, GetAjaxOptions(ajaxOptions), htmlAttributes));
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <param name="protocol">
        /// The protocol.
        /// </param>
        /// <param name="hostName">
        /// The host name.
        /// </param>
        /// <param name="fragment">
        /// The fragment.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string actionName, 
            string controllerName, 
            string protocol, 
            string hostName, 
            string fragment, 
            object routeValues, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            var newValues = new RouteValueDictionary(routeValues);
            RouteValueDictionary newAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return ActionLinkHtml5(
                ajaxHelper, 
                linkText, 
                actionName, 
                controllerName, 
                protocol, 
                hostName, 
                fragment, 
                newValues, 
                ajaxOptions, 
                newAttributes);
        }

        /// <summary>
        /// The action link html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <param name="protocol">
        /// The protocol.
        /// </param>
        /// <param name="hostName">
        /// The host name.
        /// </param>
        /// <param name="fragment">
        /// The fragment.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static MvcHtmlString ActionLinkHtml5(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string actionName, 
            string controllerName, 
            string protocol, 
            string hostName, 
            string fragment, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            if (string.IsNullOrEmpty(linkText))
            {
                throw new ArgumentException("linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(
                null /* routeName */, 
                actionName, 
                controllerName, 
                protocol, 
                hostName, 
                fragment, 
                routeValues, 
                ajaxHelper.RouteCollection, 
                ajaxHelper.ViewContext.RequestContext, 
                true /* includeImplicitMvcValues */);

            return MvcHtmlString.Create(GenerateLink(ajaxHelper, linkText, targetUrl, ajaxOptions, htmlAttributes));
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(this AjaxHelper ajaxHelper, AjaxOptions ajaxOptions)
        {
            string formAction = ajaxHelper.ViewContext.HttpContext.Request.RawUrl;

            return FormHelper(ajaxHelper, formAction, ajaxOptions, new RouteValueDictionary());
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(this AjaxHelper ajaxHelper, string actionName, AjaxOptions ajaxOptions)
        {
            return BeginFormHtml5(ajaxHelper, actionName, (string)null /* controllerName */, ajaxOptions);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(
            this AjaxHelper ajaxHelper, string actionName, object routeValues, AjaxOptions ajaxOptions)
        {
            return BeginFormHtml5(ajaxHelper, actionName, null /* controllerName */, routeValues, ajaxOptions);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
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
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(
            this AjaxHelper ajaxHelper, 
            string actionName, 
            object routeValues, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            return BeginFormHtml5(
                ajaxHelper, actionName, null /* controllerName */, routeValues, ajaxOptions, htmlAttributes);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(
            this AjaxHelper ajaxHelper, string actionName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions)
        {
            return BeginFormHtml5(ajaxHelper, actionName, null /* controllerName */, routeValues, ajaxOptions);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
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
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(
            this AjaxHelper ajaxHelper, 
            string actionName, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            return BeginFormHtml5(
                ajaxHelper, actionName, null /* controllerName */, routeValues, ajaxOptions, htmlAttributes);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="controllerName">
        /// The controller name.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(
            this AjaxHelper ajaxHelper, string actionName, string controllerName, AjaxOptions ajaxOptions)
        {
            return BeginFormHtml5(
                ajaxHelper, actionName, controllerName, null /* values */, ajaxOptions, null /* htmlAttributes */);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(
            this AjaxHelper ajaxHelper, 
            string actionName, 
            string controllerName, 
            object routeValues, 
            AjaxOptions ajaxOptions)
        {
            return BeginFormHtml5(
                ajaxHelper, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(
            this AjaxHelper ajaxHelper, 
            string actionName, 
            string controllerName, 
            object routeValues, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            var newValues = new RouteValueDictionary(routeValues);
            RouteValueDictionary newAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return BeginFormHtml5(ajaxHelper, actionName, controllerName, newValues, ajaxOptions, newAttributes);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(
            this AjaxHelper ajaxHelper, 
            string actionName, 
            string controllerName, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions)
        {
            return BeginFormHtml5(
                ajaxHelper, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        /// <summary>
        /// The begin form html 5.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
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
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginFormHtml5(
            this AjaxHelper ajaxHelper, 
            string actionName, 
            string controllerName, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            // get target URL 
            string formAction = UrlHelper.GenerateUrl(
                null, 
                actionName, 
                controllerName, 
                routeValues ?? new RouteValueDictionary(), 
                ajaxHelper.RouteCollection, 
                ajaxHelper.ViewContext.RequestContext, 
                true /* includeImplicitMvcValues */);
            if (htmlAttributes == null)
            {
                htmlAttributes = new Dictionary<string, object>();
            }

            htmlAttributes.Add("data-ajax-url", formAction);

            return FormHelper(ajaxHelper, formAction, ajaxOptions, htmlAttributes);
        }

        /// <summary>
        /// The begin route form.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="routeName">
        /// The route name.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginRouteForm(this AjaxHelper ajaxHelper, string routeName, AjaxOptions ajaxOptions)
        {
            return BeginRouteForm(ajaxHelper, routeName, null /* routeValues */, ajaxOptions, null /* htmlAttributes */);
        }

        /// <summary>
        /// The begin route form.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="routeName">
        /// The route name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginRouteForm(
            this AjaxHelper ajaxHelper, string routeName, object routeValues, AjaxOptions ajaxOptions)
        {
            return BeginRouteForm(ajaxHelper, routeName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        /// <summary>
        /// The begin route form.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="routeName">
        /// The route name.
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
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginRouteForm(
            this AjaxHelper ajaxHelper, 
            string routeName, 
            object routeValues, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            RouteValueDictionary newAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return BeginRouteForm(
                ajaxHelper, routeName, new RouteValueDictionary(routeValues), ajaxOptions, newAttributes);
        }

        /// <summary>
        /// The begin route form.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="routeName">
        /// The route name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginRouteForm(
            this AjaxHelper ajaxHelper, string routeName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions)
        {
            return BeginRouteForm(ajaxHelper, routeName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        /// <summary>
        /// The begin route form.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="routeName">
        /// The route name.
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
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        public static MvcForm BeginRouteForm(
            this AjaxHelper ajaxHelper, 
            string routeName, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            string formAction = UrlHelper.GenerateUrl(
                routeName, 
                null /* actionName */, 
                null /* controllerName */, 
                routeValues ?? new RouteValueDictionary(), 
                ajaxHelper.RouteCollection, 
                ajaxHelper.ViewContext.RequestContext, 
                false /* includeImplicitMvcValues */);
            return FormHelper(ajaxHelper, formAction, ajaxOptions, htmlAttributes);
        }

        /// <summary>
        /// The globalization script.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString GlobalizationScript(this AjaxHelper ajaxHelper)
        {
            return GlobalizationScript(ajaxHelper, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// The globalization script.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="cultureInfo">
        /// The culture info.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ajaxHelper", 
            Justification = "This is an extension method")]
        public static MvcHtmlString GlobalizationScript(this AjaxHelper ajaxHelper, CultureInfo cultureInfo)
        {
            return GlobalizationScriptHelper(AjaxHelper.GlobalizationScriptPath, cultureInfo);
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, string linkText, object routeValues, AjaxOptions ajaxOptions)
        {
            return RouteLink(
                ajaxHelper, 
                linkText, 
                null /* routeName */, 
                new RouteValueDictionary(routeValues), 
                ajaxOptions, 
                new Dictionary<string, object>());
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            object routeValues, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            return RouteLink(
                ajaxHelper, 
                linkText, 
                null /* routeName */, 
                new RouteValueDictionary(routeValues), 
                ajaxOptions, 
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, string linkText, RouteValueDictionary routeValues, AjaxOptions ajaxOptions)
        {
            return RouteLink(
                ajaxHelper, linkText, null /* routeName */, routeValues, ajaxOptions, new Dictionary<string, object>());
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            return RouteLink(ajaxHelper, linkText, null /* routeName */, routeValues, ajaxOptions, htmlAttributes);
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="routeName">
        /// The route name.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, string linkText, string routeName, AjaxOptions ajaxOptions)
        {
            return RouteLink(
                ajaxHelper, 
                linkText, 
                routeName, 
                new RouteValueDictionary(), 
                ajaxOptions, 
                new Dictionary<string, object>());
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="routeName">
        /// The route name.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string routeName, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            return RouteLink(
                ajaxHelper, 
                linkText, 
                routeName, 
                new RouteValueDictionary(), 
                ajaxOptions, 
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="routeName">
        /// The route name.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string routeName, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(), ajaxOptions, htmlAttributes);
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="routeName">
        /// The route name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, string linkText, string routeName, object routeValues, AjaxOptions ajaxOptions)
        {
            return RouteLink(
                ajaxHelper, 
                linkText, 
                routeName, 
                new RouteValueDictionary(routeValues), 
                ajaxOptions, 
                new Dictionary<string, object>());
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="routeName">
        /// The route name.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string routeName, 
            object routeValues, 
            AjaxOptions ajaxOptions, 
            object htmlAttributes)
        {
            return RouteLink(
                ajaxHelper, 
                linkText, 
                routeName, 
                new RouteValueDictionary(routeValues), 
                ajaxOptions, 
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="routeName">
        /// The route name.
        /// </param>
        /// <param name="routeValues">
        /// The route values.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string routeName, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions)
        {
            return RouteLink(
                ajaxHelper, linkText, routeName, routeValues, ajaxOptions, new Dictionary<string, object>());
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="routeName">
        /// The route name.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string routeName, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            if (string.IsNullOrEmpty(linkText))
            {
                throw new ArgumentException("linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(
                routeName, 
                null /* actionName */, 
                null /* controllerName */, 
                routeValues ?? new RouteValueDictionary(), 
                ajaxHelper.RouteCollection, 
                ajaxHelper.ViewContext.RequestContext, 
                false /* includeImplicitMvcValues */);

            return
                MvcHtmlString.Create(
                    GenerateLink(ajaxHelper, linkText, targetUrl, GetAjaxOptions(ajaxOptions), htmlAttributes));
        }

        /// <summary>
        /// The route link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="routeName">
        /// The route name.
        /// </param>
        /// <param name="protocol">
        /// The protocol.
        /// </param>
        /// <param name="hostName">
        /// The host name.
        /// </param>
        /// <param name="fragment">
        /// The fragment.
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
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static MvcHtmlString RouteLink(
            this AjaxHelper ajaxHelper, 
            string linkText, 
            string routeName, 
            string protocol, 
            string hostName, 
            string fragment, 
            RouteValueDictionary routeValues, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            if (string.IsNullOrEmpty(linkText))
            {
                throw new ArgumentException("linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(
                routeName, 
                null /* actionName */, 
                null /* controllerName */, 
                protocol, 
                hostName, 
                fragment, 
                routeValues ?? new RouteValueDictionary(), 
                ajaxHelper.RouteCollection, 
                ajaxHelper.ViewContext.RequestContext, 
                false /* includeImplicitMvcValues */);

            return
                MvcHtmlString.Create(
                    GenerateLink(ajaxHelper, linkText, targetUrl, GetAjaxOptions(ajaxOptions), htmlAttributes));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The form helper.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="formAction">
        /// The form action.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Html.MvcForm.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", 
            Justification = "You don't want to dispose of this object unless you intend to write to the response")]
        private static MvcForm FormHelper(
            this AjaxHelper ajaxHelper, 
            string formAction, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("form");
            builder.MergeAttributes(htmlAttributes);
            builder.MergeAttribute("action", "javascript:void(0)");
            builder.MergeAttribute("data-ajax-url", formAction);
            builder.MergeAttribute("method", "post");

            // builder.MergeAttribute("data-ajax", "true");
            ajaxOptions = GetAjaxOptions(ajaxOptions);

            if (ajaxHelper.ViewContext.UnobtrusiveJavaScriptEnabled)
            {
                builder.MergeAttributes(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            }

            /* else
            {
                builder.MergeAttribute("onclick", FormOnClickValue);
                builder.MergeAttribute("onsubmit", GenerateAjaxScript(ajaxOptions, FormOnSubmitFormat));
            }*/
            if (ajaxHelper.ViewContext.ClientValidationEnabled)
            {
                // forms must have an ID for client validation 
                builder.Attributes["id"] =
                    ajaxHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(Guid.NewGuid().ToString());

                // builder.GenerateId(ajaxHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(Guid.NewGuid().ToString())/*.FormIdGenerator()*/);
            }

            ajaxHelper.ViewContext.Writer.Write(builder.ToString(TagRenderMode.StartTag));
            var theForm = new MvcForm(ajaxHelper.ViewContext);

            if (ajaxHelper.ViewContext.ClientValidationEnabled)
            {
                ajaxHelper.ViewContext.FormContext.FormId = builder.Attributes["id"];
            }

            return theForm;
        }

        /// <summary>
        /// The generate link.
        /// </summary>
        /// <param name="ajaxHelper">
        /// The ajax helper.
        /// </param>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="targetUrl">
        /// The target url.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private static string GenerateLink(
            AjaxHelper ajaxHelper, 
            string linkText, 
            string targetUrl, 
            AjaxOptions ajaxOptions, 
            IDictionary<string, object> htmlAttributes)
        {
            var tag = new TagBuilder("a") { InnerHtml = HttpUtility.HtmlEncode(linkText) };

            tag.MergeAttributes(htmlAttributes);
            tag.MergeAttribute("href", "javascript:void(0)");
            tag.MergeAttribute("data-ajax-url", targetUrl);

            /*    if (ajaxHelper.ViewContext.UnobtrusiveJavaScriptEnabled)
                {*/
            IDictionary<string, object> attr = ajaxOptions.ToUnobtrusiveHtmlAttributes();

            /* attr["data-ajax"]="false";
            attr.Add("data-ajax-mobile-custom", "true");*/
            tag.MergeAttributes(attr);

            /*}
            else
            {
                tag.MergeAttribute("onclick", GenerateAjaxScript(ajaxOptions, LinkOnClickFormat));
            }*/
            return tag.ToString(TagRenderMode.Normal);
        }

        /*private static string GenerateAjaxScript(AjaxOptions ajaxOptions, string scriptFormat)
        {
            string optionsString = ajaxOptions.ToJavascriptString();
            return String.Format(CultureInfo.InvariantCulture, scriptFormat, optionsString);
        }*/

        /// <summary>
        /// The get ajax options.
        /// </summary>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.Ajax.AjaxOptions.
        /// </returns>
        private static AjaxOptions GetAjaxOptions(AjaxOptions ajaxOptions)
        {
            return (ajaxOptions != null) ? ajaxOptions : new AjaxOptions();
        }

        /// <summary>
        /// The globalization script helper.
        /// </summary>
        /// <param name="scriptPath">
        /// The script path.
        /// </param>
        /// <param name="cultureInfo">
        /// The culture info.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.MvcHtmlString.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        private static MvcHtmlString GlobalizationScriptHelper(string scriptPath, CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                throw new ArgumentNullException("cultureInfo");
            }

            string src = VirtualPathUtility.AppendTrailingSlash(scriptPath) + cultureInfo.Name + ".js";
            string scriptWithCorrectNewLines = _globalizationScript.Replace("\r\n", Environment.NewLine);
            string formatted = string.Format(CultureInfo.InvariantCulture, scriptWithCorrectNewLines, src);

            return MvcHtmlString.Create(formatted);
        }

        #endregion
    }
}