using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using NUnit.Framework;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;

namespace WB.Tests.Unit.Designer.Code
{
    [TestFixture]
    public class PagingTests
    {
        private static string RenderPager(int current, int total, int slides = 2)
        {
            var helper = new Mock<IHtmlHelper>().Object;
            var content = helper.Pager(current, total, i => "/page/" + i, slides);
            using var writer = new StringWriter();
            content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        [Test]
        public void GetPage_should_skip_and_take()
        {
            Enumerable.Range(1, 10).GetPage(1, 3).Should().Equal(4, 5, 6);
        }

        [Test]
        public void ToPagedList_should_use_one_based_page_numbers()
        {
            var list = Enumerable.Range(1, 10).ToPagedList(2, 4);

            list.Should().Equal(5, 6, 7, 8);
            list.PageIndex.Should().Be(1);
            list.PageSize.Should().Be(4);
            list.TotalCount.Should().Be(10);
            list.TotalPages.Should().Be(3);
        }

        [Test]
        public void PagedList_should_report_navigation_flags()
        {
            var first = Enumerable.Range(1, 10).ToPagedList(1, 4);
            var last = Enumerable.Range(1, 10).ToPagedList(3, 4);

            first.HasPreviousPage.Should().BeFalse();
            first.HasNextPage.Should().BeTrue();
            last.HasPreviousPage.Should().BeTrue();
            last.HasNextPage.Should().BeFalse();
        }

        [Test]
        public void PagedList_with_exact_multiple_should_not_add_extra_page()
        {
            Enumerable.Range(1, 8).ToPagedList(1, 4).TotalPages.Should().Be(2);
        }

        [Test]
        public void PagedList_with_explicit_total_should_not_page_source()
        {
            var list = new[] { 1, 2 }.ToPagedList(3, 2, 100);

            list.Should().Equal(1, 2);
            list.TotalCount.Should().Be(100);
            list.TotalPages.Should().Be(50);
            list.PageIndex.Should().Be(2);
        }

        [Test]
        public void PagedList_with_zero_page_size_should_be_empty()
        {
            var list = new PagedList<int>(new[] { 1, 2 }, 0, 0, 2);

            list.Should().BeEmpty();
            list.TotalPages.Should().Be(0);
        }

        [Test]
        public void Pager_should_render_nothing_for_single_page()
        {
            RenderPager(1, 1).Should().BeEmpty();
            RenderPager(1, 0).Should().BeEmpty();
        }

        [Test]
        public void Pager_should_mark_current_page_active_and_link_others()
        {
            var html = RenderPager(2, 3);

            html.Should().Contain("<li class=\"active\"><span class=\"currentPage\">2</span></li>");
            html.Should().Contain("href=\"/page/1\"");
            html.Should().Contain("href=\"/page/3\"");
        }

        [Test]
        public void Pager_should_disable_first_and_previous_on_first_page()
        {
            var html = RenderPager(1, 5);

            html.Should().Contain("disabledPage");
            html.Should().NotContain("href=\"/page/0\"");
            html.Should().Contain("href=\"/page/2\"");
        }

        [Test]
        public void Pager_should_show_ellipses_for_long_ranges()
        {
            var html = RenderPager(10, 20, 1);

            html.Should().Contain("ellipses");
            html.Should().Contain("href=\"/page/20\"");
            html.Should().Contain("href=\"/page/1\"");
        }

        [Test]
        public void Pager_should_throw_when_url_is_null_for_non_active_page()
        {
            var helper = new Mock<IHtmlHelper>().Object;
            Action act = () => helper.Pager(1, 3, _ => null, 2);
            act.Should().Throw<InvalidOperationException>();
        }
    }
}

