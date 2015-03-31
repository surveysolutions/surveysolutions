using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/*
<!-- Usage in razor (note @model): -->
@using BootstrapSupport
@model IPagedList

@Html.Pager(Model.PageIndex,
            Model.TotalPages,
            x => Url.Action("Index", new {page = x}),
            " pagination-right")

// Index action on the HomeController from the sample project:
public ActionResult Index(int page = 1)
{
    var pageSize = 3;
    var homeInputModels = _models;
    return View(homeInputModels.ToPagedList(page, pageSize));
}
*/

namespace WB.UI.Designer.BootstrapSupport.HtmlHelpers
{
    public static class PaginiationHelperExtensions
    {
        public static MvcHtmlString Pager(this HtmlHelper helper,
            int currentPage, int totalPages, 
            Func<int, string> pageUrl, 
            int pageSlides,
            string additionalPagerCssClass = "")
        {
            if (totalPages <= 1)
                return MvcHtmlString.Empty;

            var nav = new TagBuilder("nav");

            var pagination = new TagBuilder("ul");
            pagination.AddCssClass("pagination");
            var pager = new TagBuilder("ul");
            pager.AddCssClass("pager");

            var pageFrom = Math.Max(1, currentPage - pageSlides);
            var pageTo = Math.Min(totalPages, currentPage + pageSlides);
            pageFrom = Math.Max(1, Math.Min(pageTo - 2 * pageSlides, pageFrom));
            pageTo = Math.Min(totalPages, Math.Max(pageFrom + 2 * pageSlides, pageTo));


            if (pageFrom > 1)
            {
                MakePagingItem(isActive: false, text: pageUrl(1), htmlTitle: "1", root: pagination);
            }

            if (pageFrom > 2 )
            {
                MakeDisabledPagingItem(htmlTitle: "&hellip;", root: pagination, additionalClass: "ellipses");
            }

            for (var i = pageFrom; i <= pageTo; i++)
            {
                MakePagingItem(isActive: i == currentPage, text: pageUrl(i), htmlTitle: i.ToString(), root: pagination);
            }

            if (pageTo + 1 < totalPages)
            {
                MakeDisabledPagingItem(htmlTitle: "&hellip;", root: pagination, additionalClass: "ellipses");
            }

            if (pageTo < totalPages)
            {
                MakePagingItem(isActive: false, text: pageUrl(totalPages), htmlTitle: totalPages.ToString(), root: pagination);
            }

            if (currentPage > 1)
            {
                MakePagingItem(isActive: false, text: pageUrl(1), htmlTitle: "FIRST", root: pager);
                MakePagingItem(isActive: false, text: pageUrl(currentPage - 1), htmlTitle: "PREVIOUS", root: pager);
            }
            else
            {
                MakeDisabledPagingItem(htmlTitle: "FIRST", root: pager);
                MakeDisabledPagingItem(htmlTitle: "PREVIOUS", root: pager); 
            }

            if (currentPage < totalPages)
            {
                MakePagingItem(isActive: false, text: pageUrl(currentPage + 1), htmlTitle: "NEXT", root: pager);
                MakePagingItem(isActive: false, text: pageUrl(totalPages), htmlTitle: "LAST", root: pager);
            }
            else
            {
                MakeDisabledPagingItem(htmlTitle: "NEXT", root: pager);
                MakeDisabledPagingItem(htmlTitle: "LAST", root: pager);
            }

            nav.InnerHtml = pagination.ToString() + pager.ToString();

            return MvcHtmlString.Create(nav.ToString());
        }

        private static void MakeDisabledPagingItem(string htmlTitle, TagBuilder root, string additionalClass = null)
        {
            var li = new TagBuilder("li");
            li.AddCssClass("disabled");
            if (!string.IsNullOrWhiteSpace(additionalClass))
            {
                li.AddCssClass(additionalClass);
            }
            var a = new TagBuilder("a");
            a.MergeAttribute("href", "#");
            a.InnerHtml = htmlTitle;
            a.AddCssClass("disabledPage");

            li.InnerHtml = a.ToString();
            root.InnerHtml += li;
        }

        private static void MakePagingItem(bool isActive, string text, string htmlTitle, TagBuilder root)
        {
            var li = new TagBuilder("li");

            if (isActive)
            {
                li.AddCssClass("active");
                var span = new TagBuilder("span");
                span.AddCssClass("currentPage");
                span.InnerHtml = htmlTitle;

                li.InnerHtml += span;
            }
            else
            {
                var a = new TagBuilder("a");
                a.MergeAttribute("href", text);
                a.InnerHtml = htmlTitle;
                li.InnerHtml += a;    
            }
                
            root.InnerHtml += li;
        }
    }

    public interface IPagedList : IEnumerable
    {
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }

    public interface IPagedList<T> : IPagedList, IList<T>
    {
    }

    public class PagedList<T> : List<T>, IPagedList<T>
    {
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize) :
            this(source.GetPage(pageIndex, pageSize), pageIndex, pageSize, source.Count()) { }

        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            if (pageSize == 0) return;

            this.TotalCount = totalCount;
            this.TotalPages = totalCount / pageSize;

            if (totalCount % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;

            this.AddRange(source.ToList());
        }

        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }

        public bool HasPreviousPage { get { return (PageIndex > 0); } }
        public bool HasNextPage { get { return (PageIndex + 1 < TotalPages); } }
    }

    public static class PagingExtensions
    {
        public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> query, int page, int pageSize)
        {
            return new PagedList<T>(query, page - 1, pageSize);
        }

        public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> query, int page, int pageSize, int totalCount)
        {
            return new PagedList<T>(query, page - 1, pageSize, totalCount);
        }

        public static IEnumerable<T> GetPage<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
        {
            return source.Skip(pageIndex*pageSize).Take(pageSize);
        }
        
        // You can create your own paging extension that delegates to your
        // persistence layer such as NHibernate or Entity Framework.
        // This is an example how an `IPagedList<T>` can be created from 
        // an `IRavenQueryable<T>`:        
        /*
        public static IPagedList<T> ToPagedList<T>(this IRavenQueryable<T> query, int page, int pageSize)
        {
            RavenQueryStatistics stats;
            var q2 = query.Statistics(out stats)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            var list = new PagedList<T>(
                            q2,
                            page - 1,
                            pageSize,
                            stats.TotalResults
                        );
            return list;
        }
        */
    }   
}