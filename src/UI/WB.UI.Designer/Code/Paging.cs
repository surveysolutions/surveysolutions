using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using WB.UI.Designer.Resources;

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
        public static IHtmlContent Pager(this IHtmlHelper helper,
            int currentPage, int totalPages, 
            Func<int, string?> pageUrl, 
            int pageSlides,
            string additionalPagerCssClass = "")
        {
            if (totalPages <= 1)
                return new HtmlString(string.Empty);

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
                MakePagingItem(isActive: false, text: pageUrl(1), htmlTitle: Paging.First.ToUpper(), root: pager);
                MakePagingItem(isActive: false, text: pageUrl(currentPage - 1), htmlTitle: Paging.Previous.ToUpper(), root: pager);
            }
            else
            {
                MakeDisabledPagingItem(htmlTitle: Paging.First.ToUpper(), root: pager);
                MakeDisabledPagingItem(htmlTitle: Paging.Previous.ToUpper(), root: pager); 
            }

            if (currentPage < totalPages)
            {
                MakePagingItem(isActive: false, text: pageUrl(currentPage + 1), htmlTitle: Paging.Next.ToUpper(), root: pager);
                MakePagingItem(isActive: false, text: pageUrl(totalPages), htmlTitle: Paging.Last.ToUpper(), root: pager);
            }
            else
            {
                MakeDisabledPagingItem(htmlTitle: Paging.Next.ToUpper(), root: pager);
                MakeDisabledPagingItem(htmlTitle: Paging.Last.ToUpper(), root: pager);
            }

            nav.InnerHtml.AppendHtml(pagination);
            nav.InnerHtml.AppendHtml(pager);

            return nav;
        }

        private static void MakeDisabledPagingItem(string htmlTitle, TagBuilder root, string? additionalClass = null)
        {
            var li = new TagBuilder("li");
            li.AddCssClass("disabled");
            if (!string.IsNullOrWhiteSpace(additionalClass))
            {
                li.AddCssClass(additionalClass);
            }
            var a = new TagBuilder("a");
            a.MergeAttribute("href", "#");
            a.InnerHtml.AppendHtml(htmlTitle);
            a.AddCssClass("disabledPage");

            li.InnerHtml.AppendHtml(a);
            root.InnerHtml.AppendHtml(li);
        }

        private static void MakePagingItem(bool isActive, string? text, string htmlTitle, TagBuilder root)
        {
            var li = new TagBuilder("li");

            if (isActive)
            {
                li.AddCssClass("active");
                var span = new TagBuilder("span");
                span.AddCssClass("currentPage");
                span.InnerHtml.AppendHtml(htmlTitle);

                li.InnerHtml.AppendHtml(span);
            }
            else
            {
                if (text == null) throw new InvalidOperationException("Page text is null");
                
                var a = new TagBuilder("a");
                a.MergeAttribute("href", text);
                a.InnerHtml.AppendHtml(htmlTitle);
                li.InnerHtml.AppendHtml(a);
            }
                
            root.InnerHtml.AppendHtml(li);
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
    }   
}
