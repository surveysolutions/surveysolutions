using FluentAssertions;
using NUnit.Framework;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.Tests.Web.Headquarters.Workspaces
{
    [TestOf(typeof(WorkspacesListFilter))]
    public class WorkspacesListFilterTests
    {
        [Test]
        public void should_preserve_default_and_large_page_sizes()
        {
            new WorkspacesListFilter().Length.Should().Be(20);
            new WorkspacesListFilter { Length = 1000 }.Length.Should().Be(1000);
        }
    }
}
