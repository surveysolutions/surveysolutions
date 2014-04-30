namespace Web.Supervisor.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// The control group.
    /// </summary>
    public class ControlGroup : IDisposable
    {
        #region Fields

        /// <summary>
        /// The _html.
        /// </summary>
        private readonly HtmlHelper _html;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlGroup"/> class.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        public ControlGroup(HtmlHelper html)
        {
            this._html = html;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this._html.ViewContext.Writer.Write(this._html.EndControlGroup());
        }

        #endregion
    }

    /// <summary>
    /// The control group extensions.
    /// </summary>
    public static class ControlGroupExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The begin control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="modelProperty">
        /// The model property.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IHtmlString"/>.
        /// </returns>
        public static IHtmlString BeginControlGroupFor<T>(
            this HtmlHelper<T> html, Expression<Func<T, object>> modelProperty)
        {
            return BeginControlGroupFor(html, modelProperty, null);
        }

        /// <summary>
        /// The begin control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="modelProperty">
        /// The model property.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IHtmlString"/>.
        /// </returns>
        public static IHtmlString BeginControlGroupFor<T>(
            this HtmlHelper<T> html, Expression<Func<T, object>> modelProperty, object htmlAttributes)
        {
            return BeginControlGroupFor(html, modelProperty, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// The begin control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="modelProperty">
        /// The model property.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IHtmlString"/>.
        /// </returns>
        public static IHtmlString BeginControlGroupFor<T>(
            this HtmlHelper<T> html, 
            Expression<Func<T, object>> modelProperty, 
            IDictionary<string, object> htmlAttributes)
        {
            string propertyName = ExpressionHelper.GetExpressionText(modelProperty);
            return BeginControlGroupFor(html, propertyName, null);
        }

        /// <summary>
        /// The begin control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IHtmlString"/>.
        /// </returns>
        public static IHtmlString BeginControlGroupFor<T>(this HtmlHelper<T> html, string propertyName)
        {
            return BeginControlGroupFor(html, propertyName, null);
        }

        /// <summary>
        /// The begin control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IHtmlString"/>.
        /// </returns>
        public static IHtmlString BeginControlGroupFor<T>(
            this HtmlHelper<T> html, string propertyName, object htmlAttributes)
        {
            return BeginControlGroupFor(html, propertyName, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// The begin control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IHtmlString"/>.
        /// </returns>
        public static IHtmlString BeginControlGroupFor<T>(
            this HtmlHelper<T> html, string propertyName, IDictionary<string, object> htmlAttributes)
        {
            var controlGroupWrapper = new TagBuilder("div");
            controlGroupWrapper.MergeAttributes(htmlAttributes);
            controlGroupWrapper.AddCssClass("control-group");
            string partialFieldName = propertyName;
            string fullHtmlFieldName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(partialFieldName);
            if (!html.ViewData.ModelState.IsValidField(fullHtmlFieldName))
            {
                controlGroupWrapper.AddCssClass("error");
            }

            string openingTag = controlGroupWrapper.ToString(TagRenderMode.StartTag);
            return MvcHtmlString.Create(openingTag);
        }

        /// <summary>
        /// The control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="modelProperty">
        /// The model property.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ControlGroup"/>.
        /// </returns>
        public static ControlGroup ControlGroupFor<T>(
            this HtmlHelper<T> html, Expression<Func<T, object>> modelProperty)
        {
            return ControlGroupFor(html, modelProperty, null);
        }

        /// <summary>
        /// The control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="modelProperty">
        /// The model property.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ControlGroup"/>.
        /// </returns>
        public static ControlGroup ControlGroupFor<T>(
            this HtmlHelper<T> html, Expression<Func<T, object>> modelProperty, object htmlAttributes)
        {
            string propertyName = ExpressionHelper.GetExpressionText(modelProperty);
            return ControlGroupFor(html, propertyName, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// The control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ControlGroup"/>.
        /// </returns>
        public static ControlGroup ControlGroupFor<T>(this HtmlHelper<T> html, string propertyName)
        {
            return ControlGroupFor(html, propertyName, null);
        }

        /// <summary>
        /// The control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ControlGroup"/>.
        /// </returns>
        public static ControlGroup ControlGroupFor<T>(
            this HtmlHelper<T> html, string propertyName, object htmlAttributes)
        {
            return ControlGroupFor(html, propertyName, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// The control group for.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ControlGroup"/>.
        /// </returns>
        public static ControlGroup ControlGroupFor<T>(
            this HtmlHelper<T> html, string propertyName, IDictionary<string, object> htmlAttributes)
        {
            html.ViewContext.Writer.Write(BeginControlGroupFor(html, propertyName, htmlAttributes));
            return new ControlGroup(html);
        }

        /// <summary>
        /// The end control group.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <returns>
        /// The <see cref="IHtmlString"/>.
        /// </returns>
        public static IHtmlString EndControlGroup(this HtmlHelper html)
        {
            return MvcHtmlString.Create("</div>");
        }

        #endregion
    }

    /// <summary>
    /// The alerts.
    /// </summary>
    public static class Alerts
    {
        #region Constants

        /// <summary>
        /// The attention.
        /// </summary>
        public const string ATTENTION = "attention";

        /// <summary>
        /// The error.
        /// </summary>
        public const string ERROR = "error";

        /// <summary>
        /// The information.
        /// </summary>
        public const string INFORMATION = "info";

        /// <summary>
        /// The success.
        /// </summary>
        public const string SUCCESS = "success";

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the all.
        /// </summary>
        public static string[] ALL
        {
            get
            {
                return new[] { SUCCESS, ATTENTION, INFORMATION, ERROR };
            }
        }

        #endregion
    }
}