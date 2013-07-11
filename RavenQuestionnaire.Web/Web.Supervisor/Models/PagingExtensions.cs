namespace Web.Supervisor.Models
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Responsible for added new properties for QPager
    /// </summary>
    public static class PagingExtensions
    {
        public static HtmlString QPager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount)
        {
            return QPager(htmlHelper, pageSize, currentPage, totalItemCount, null, null);
        }

        public static HtmlString QPager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount, string actionName)
        {
            return QPager(htmlHelper, pageSize, currentPage, totalItemCount, actionName, null);
        }

        public static HtmlString QPager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount, object values)
        {
            return QPager(htmlHelper, pageSize, currentPage, totalItemCount, null, new RouteValueDictionary(values));
        }

        public static HtmlString QPager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount, string actionName, object values)
        {
            return QPager(htmlHelper, pageSize, currentPage, totalItemCount, actionName, new RouteValueDictionary(values));
        }

        public static HtmlString QPager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount, RouteValueDictionary valuesDictionary)
        {
            return QPager(htmlHelper, pageSize, currentPage, totalItemCount, null, valuesDictionary);
        }

        public static HtmlString QPager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount, string actionName, RouteValueDictionary valuesDictionary)
        {
            if (valuesDictionary == null)
            {
                valuesDictionary = new RouteValueDictionary();
            }
            if (actionName != null)
            {
                if (valuesDictionary.ContainsKey("action"))
                {
                    throw new ArgumentException("The valuesDictionary already contains an action.", "actionName");
                }
                valuesDictionary.Add("action", actionName);
            }
            var pager = new QPager(htmlHelper.ViewContext, pageSize, currentPage, totalItemCount, valuesDictionary, null);
            return pager.RenderHtml();
        }
    }
}