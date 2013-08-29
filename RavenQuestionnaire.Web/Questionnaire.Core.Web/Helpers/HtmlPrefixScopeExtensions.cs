namespace Questionnaire.Core.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// The html prefix scope extensions.
    /// </summary>
    public static class HtmlPrefixScopeExtensions
    {
        #region Constants

        /// <summary>
        /// The ids to reuse key.
        /// </summary>
        private const string idsToReuseKey = "__htmlPrefixScopeExtensions_IdsToReuse_";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The begin collection item.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="collectionName">
        /// The collection name.
        /// </param>
        /// <returns>
        /// The System.IDisposable.
        /// </returns>
        public static IDisposable BeginCollectionItem(this HtmlHelper html, string collectionName)
        {
            return BeginCollectionItem(html, collectionName, false, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// The begin collection item.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="collectionName">
        /// The collection name.
        /// </param>
        /// <param name="uniqueIdentifire">
        /// The unique identifire.
        /// </param>
        /// <returns>
        /// The System.IDisposable.
        /// </returns>
        public static IDisposable BeginCollectionItem(
            this HtmlHelper html, string collectionName, object uniqueIdentifire, bool includeIndex = true)
        {
            return BeginCollectionItem(html, collectionName, false, uniqueIdentifire, includeIndex);
        }

        /// <summary>
        /// The begin collection item.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="collectionName">
        /// The collection name.
        /// </param>
        /// <param name="includePreviusPrefix">
        /// The include previus prefix.
        /// </param>
        /// <param name="uniqueIdentifire">
        /// The unique identifire.
        /// </param>
        /// <param name="includeIndex">
        /// The include index tag or not
        /// </param>
        /// <returns>
        /// The System.IDisposable.
        /// </returns>
        public static IDisposable BeginCollectionItem(
            this HtmlHelper html, string collectionName, bool includePreviusPrefix, object uniqueIdentifire, bool includeIndex = true)
        {
            Queue<string> idsToReuse = GetIdsToReuse(html.ViewContext.HttpContext, collectionName);
            string itemIndex = idsToReuse.Count > 0 ? idsToReuse.Dequeue() : uniqueIdentifire.ToString();

            if (includeIndex)
            {
                // autocomplete="off" is needed to work around a very annoying Chrome behaviour whereby it reuses old values after the user clicks "Back", which causes the xyz.index and xyz[...] values to get out of sync.
                html.ViewContext.Writer.WriteLine(
                    string.Format(
                        "<input type=\"hidden\" name=\"{0}.index\" autocomplete=\"off\" value=\"{1}\" />",
                        html.GetHtmlPrefix(collectionName, includePreviusPrefix),
                        html.Encode(itemIndex)));
            }
            return BeginHtmlFieldPrefixScope(
                html, string.Format("{0}[{1}]", collectionName, itemIndex), includePreviusPrefix);
        }

        /// <summary>
        /// The begin html field prefix scope.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="htmlFieldPrefix">
        /// The html field prefix.
        /// </param>
        /// <returns>
        /// The System.IDisposable.
        /// </returns>
        public static IDisposable BeginHtmlFieldPrefixScope(this HtmlHelper html, string htmlFieldPrefix)
        {
            return BeginHtmlFieldPrefixScope(html, htmlFieldPrefix, false);
        }

        /// <summary>
        /// The begin html field prefix scope.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="htmlFieldPrefix">
        /// The html field prefix.
        /// </param>
        /// <param name="includePreviusPrefix">
        /// The include previus prefix.
        /// </param>
        /// <returns>
        /// The System.IDisposable.
        /// </returns>
        public static IDisposable BeginHtmlFieldPrefixScope(
            this HtmlHelper html, string htmlFieldPrefix, bool includePreviusPrefix)
        {
            return new HtmlFieldPrefixScope(
                html.ViewData.TemplateInfo, html.GetHtmlPrefix(htmlFieldPrefix, includePreviusPrefix));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get html prefix.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <param name="htmlFieldPrefix">
        /// The html field prefix.
        /// </param>
        /// <param name="includePreviusPrefix">
        /// The include previus prefix.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private static string GetHtmlPrefix(this HtmlHelper html, string htmlFieldPrefix, bool includePreviusPrefix)
        {
            string previousPrefix = string.IsNullOrEmpty(html.ViewData.TemplateInfo.HtmlFieldPrefix)
                                        ? string.Empty
                                        : html.ViewData.TemplateInfo.HtmlFieldPrefix + ".";
            return includePreviusPrefix ? previousPrefix + htmlFieldPrefix : htmlFieldPrefix;
        }

        /// <summary>
        /// The get ids to reuse.
        /// </summary>
        /// <param name="httpContext">
        /// The http context.
        /// </param>
        /// <param name="collectionName">
        /// The collection name.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.Queue`1[T -&gt; System.String].
        /// </returns>
        private static Queue<string> GetIdsToReuse(HttpContextBase httpContext, string collectionName)
        {
            // We need to use the same sequence of IDs following a server-side validation failure,  
            // otherwise the framework won't render the validation error messages next to each item.
            string key = idsToReuseKey + collectionName;
            var queue = (Queue<string>)httpContext.Items[key];
            if (queue == null)
            {
                httpContext.Items[key] = queue = new Queue<string>();
                string previouslyUsedIds = httpContext.Request[collectionName + ".index"];
                if (!string.IsNullOrEmpty(previouslyUsedIds))
                {
                    foreach (string previouslyUsedId in previouslyUsedIds.Split(','))
                    {
                        queue.Enqueue(previouslyUsedId);
                    }
                }
            }

            return queue;
        }

        #endregion

        /// <summary>
        /// The html field prefix scope.
        /// </summary>
        private class HtmlFieldPrefixScope : IDisposable
        {
            #region Fields

            /// <summary>
            /// The previous html field prefix.
            /// </summary>
            private readonly string previousHtmlFieldPrefix;

            /// <summary>
            /// The template info.
            /// </summary>
            private readonly TemplateInfo templateInfo;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="HtmlFieldPrefixScope"/> class.
            /// </summary>
            /// <param name="templateInfo">
            /// The template info.
            /// </param>
            /// <param name="htmlFieldPrefix">
            /// The html field prefix.
            /// </param>
            public HtmlFieldPrefixScope(TemplateInfo templateInfo, string htmlFieldPrefix)
            {
                this.templateInfo = templateInfo;

                this.previousHtmlFieldPrefix = templateInfo.HtmlFieldPrefix;
                templateInfo.HtmlFieldPrefix = htmlFieldPrefix;
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The dispose.
            /// </summary>
            public void Dispose()
            {
                this.templateInfo.HtmlFieldPrefix = this.previousHtmlFieldPrefix;
            }

            #endregion
        }
    }
}