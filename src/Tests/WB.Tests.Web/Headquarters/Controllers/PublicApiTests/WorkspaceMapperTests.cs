using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    [TestOf(typeof(WorkspaceMapper))]
    public class WorkspaceMapperTests
    {
        [Test]
        public void ToApiView_should_map_Name()
        {
            var workspace = new Workspace("primary", "Primary workspace", null);
            var result = workspace.ToApiView();
            Assert.That(result.Name, Is.EqualTo("primary"));
        }

        [Test]
        public void ToApiView_should_map_DisplayName()
        {
            var workspace = new Workspace("primary", "Primary workspace", null);
            var result = workspace.ToApiView();
            Assert.That(result.DisplayName, Is.EqualTo("Primary workspace"));
        }

        [Test]
        public void ToApiView_should_map_CreatedAtUtc()
        {
            var created = new DateTime(2024, 3, 1, 12, 0, 0, DateTimeKind.Utc);
            var workspace = new Workspace("primary", "Primary workspace", created);
            var result = workspace.ToApiView();
            Assert.That(result.CreatedAtUtc, Is.EqualTo(created));
        }

        [Test]
        public void ToApiView_should_map_DisabledAtUtc_as_null_for_active_workspace()
        {
            var workspace = new Workspace("primary", "Primary workspace", null);
            var result = workspace.ToApiView();
            Assert.That(result.DisabledAtUtc, Is.Null);
        }

        [Test]
        public void ToApiView_should_map_DisabledAtUtc_when_workspace_is_disabled()
        {
            var workspace = new Workspace("secondary", "Secondary workspace", null);
            workspace.Disable();
            var result = workspace.ToApiView();
            Assert.That(result.DisabledAtUtc, Is.Not.Null);
        }
    }
}
