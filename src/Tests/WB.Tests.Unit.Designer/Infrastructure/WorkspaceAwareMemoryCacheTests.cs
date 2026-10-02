using FluentAssertions;
using NUnit.Framework;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Tests.Unit.Designer.Infrastructure
{
    [TestFixture]
    [TestOf(typeof(WorkspaceAwareMemoryCache))]
    public class WorkspaceAwareMemoryCacheTests
    {
        [Test]
        public void when_same_workspace_requested_twice_should_return_same_cache()
        {
            var source = new WorkspaceAwareMemoryCache();

            source.GetCache("a").Should().BeSameAs(source.GetCache("a"));
        }

        [Test]
        public void when_different_workspaces_should_return_isolated_caches()
        {
            var source = new WorkspaceAwareMemoryCache();
            using (var entry = source.GetCache("a").CreateEntry("key"))
                entry.Value = 1;

            source.GetCache("b").TryGetValue("key", out _).Should().BeFalse();
            source.GetCache("a").TryGetValue("key", out object value).Should().BeTrue();
            value.Should().Be(1);
        }
    }
}



