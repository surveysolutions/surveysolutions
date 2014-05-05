namespace Web.Supervisor.Models
{
    using System;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Routing;

    /// <summary>
    /// Creating Html element QPager
    /// </summary>
    public class QPager
    {
        #region Fields

        /// <summary>
        /// ViewContext object
        /// </summary>
        private ViewContext viewContext;

        /// <summary>
        /// The Page Size
        /// </summary>
        private readonly int pageSize;

        /// <summary>
        /// The Current Page
        /// </summary>
        private readonly int currentPage;

        /// <summary>
        /// The Total Item Count
        /// </summary>
        private readonly int totalItemCount;

        /// <summary>
        /// The Link Without Page Values Dictionary
        /// </summary>
        private readonly RouteValueDictionary linkWithoutPageValuesDictionary;

        /// <summary>
        /// The Ajax Options
        /// </summary>
        private readonly AjaxOptions ajaxOptions;

        #endregion

        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="QPager"/> class.
        /// </summary>
        /// <param name="viewContext">
        /// The view context.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="currentPage">
        /// The current page.
        /// </param>
        /// <param name="totalItemCount">
        /// The total item count.
        /// </param>
        /// <param name="valuesDictionary">
        /// The values dictionary.
        /// </param>
        /// <param name="ajaxOptions">
        /// The ajax options.
        /// </param>
        public QPager(ViewContext viewContext, int pageSize, int currentPage, int totalItemCount, RouteValueDictionary valuesDictionary, AjaxOptions ajaxOptions)
        {
            this.viewContext = viewContext;
            this.pageSize = pageSize;
            this.currentPage = currentPage;
            this.totalItemCount = totalItemCount;
            this.linkWithoutPageValuesDictionary = valuesDictionary;
            this.ajaxOptions = ajaxOptions;
        }

        #endregion

        #region Public

        /// <summary>
        /// Responsible for creation new html element
        /// </summary>
        /// <returns>
        /// new Html element
        /// </returns>
        public HtmlString RenderHtml()
        {
            var pageCount = (int)Math.Ceiling(this.totalItemCount / (double)this.pageSize);
            const int nrOfPagesToDisplay = 10;
            var sb = new StringBuilder();
            sb.Append("<ul>");
            // Previous
            sb.AppendFormat("<li  class=\"prev {0}\">", this.currentPage > 1 ? string.Empty : "disabled");
            sb.Append(this.currentPage > 1 ? this.GeneratePageLink("&larr;", this.currentPage - 1) : "<a>&larr;</a>");
            sb.Append("</li>");
            var start = 1;
            var end = pageCount;
            if (pageCount > nrOfPagesToDisplay)
            {
                var middle = (int)Math.Ceiling(nrOfPagesToDisplay / 2d) - 1;
                var below = (this.currentPage - middle);
                var above = (this.currentPage + middle);
                if (below < 4)
                {
                    above = nrOfPagesToDisplay;
                    below = 1;
                }
                else if (above > (pageCount - 4))
                {
                    above = pageCount;
                    below = (pageCount - nrOfPagesToDisplay);
                }

                start = below;
                end = above;
            }

            if (start > 3)
            {
                sb.AppendFormat("<li>");
                sb.Append(this.GeneratePageLink("1", 1));
                sb.AppendFormat("</li>");
                sb.AppendFormat("<li>");
                sb.Append(this.GeneratePageLink("2", 2));
                sb.AppendFormat("</li>");
                sb.AppendFormat("<li class=\"disabled\">");
                sb.Append("...");
                sb.AppendFormat("</li>");
            }

            for (var i = start; i <= end; i++)
            {
                if (i == this.currentPage || (this.currentPage <= 0 && i == 0))
                {
                    sb.AppendFormat("<li class=\"active\">");
                    sb.AppendFormat("<a>{0}</a>", i);
                    sb.AppendFormat("</li>");
                }
                else
                {
                    sb.AppendFormat("<li>");
                    sb.Append(this.GeneratePageLink(i.ToString(), i));
                    sb.AppendFormat("</li>");
                }
            }

            if (end < (pageCount - 3))
            {
                sb.AppendFormat("<li class=\"disabled\">");
                sb.Append("...");
                sb.AppendFormat("</li>");
                sb.AppendFormat("<li>");
                sb.Append(this.GeneratePageLink((pageCount - 1).ToString(), pageCount - 1));
                sb.AppendFormat("</li>");
                sb.AppendFormat("<li>");
                sb.Append(this.GeneratePageLink(pageCount.ToString(), pageCount));
                sb.AppendFormat("</li>");
            }

            // Next
            sb.AppendFormat("<li  class=\"next {0}\">", this.currentPage < pageCount ? string.Empty : "disabled");
            sb.Append(this.currentPage < pageCount ? this.GeneratePageLink("&rarr;", (this.currentPage + 1)) : "<a>&rarr;</a>");
            sb.Append("</ul>");
            return new HtmlString(sb.ToString());
        }

        #endregion

        #region Private

        /// <summary>
        /// Responsible for generation link for number page
        /// </summary>
        /// <param name="linkText">
        /// The link text.
        /// </param>
        /// <param name="pageNumber">
        /// The page number.
        /// </param>
        /// <returns>
        /// link for number page
        /// </returns>
        private string GeneratePageLink(string linkText, int pageNumber)
        {
            var routeDataValues = this.viewContext.RequestContext.RouteData.Values;
            RouteValueDictionary pageLinkValueDictionary;
            // Avoid canonical errors when page count is equal to 1.
            if (pageNumber == 1)
            {
                pageLinkValueDictionary = new RouteValueDictionary(this.linkWithoutPageValuesDictionary);
                if (routeDataValues.ContainsKey("page"))
                {
                    routeDataValues.Remove("page");
                }
            }
            else
            {
                pageLinkValueDictionary = new RouteValueDictionary(this.linkWithoutPageValuesDictionary) { { "page", pageNumber } };
            }

            // To be sure we get the right route, ensure the controller and action are specified.
            if (!pageLinkValueDictionary.ContainsKey("controller") && routeDataValues.ContainsKey("controller"))
            {
                pageLinkValueDictionary.Add("controller", routeDataValues["controller"]);
            }

            if (!pageLinkValueDictionary.ContainsKey("action") && routeDataValues.ContainsKey("action"))
            {
                pageLinkValueDictionary.Add("action", routeDataValues["action"]);
            }

            // 'Render' virtual path.
            var virtualPathForArea = RouteTable.Routes.GetVirtualPathForArea(this.viewContext.RequestContext, pageLinkValueDictionary);

            if (virtualPathForArea == null)
                return null;

            var stringBuilder = new StringBuilder("<a");

            if (this.ajaxOptions != null)
                foreach (var ajaxOption in this.ajaxOptions.ToUnobtrusiveHtmlAttributes())
                    stringBuilder.AppendFormat(" {0}=\"{1}\"", ajaxOption.Key, ajaxOption.Value);

            stringBuilder.AppendFormat(" href=\"{0}\">{1}</a>", virtualPathForArea.VirtualPath, linkText);

            return stringBuilder.ToString();
        }

        #endregion
    }
}