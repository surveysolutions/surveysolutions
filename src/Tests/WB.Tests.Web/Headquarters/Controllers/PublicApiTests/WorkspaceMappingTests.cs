using System;
using AutoMapper;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.Tests.Web.Headquarters.Controllers.PublicApiTests
{
    [TestOf(typeof(WorkspacePublicApiMapProfile))]
    public class WorkspaceMappingTests
    {
        private IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<WorkspacePublicApiMapProfile>();
            }).CreateMapper();
        }

        [Test]
        public void when_mapping_Workspace_to_WorkspaceApiView_should_map_Name()
        {
            var workspace = new Workspace("myworkspace", "My Workspace", DateTime.UtcNow);

            var view = mapper.Map<WorkspaceApiView>(workspace);

            Assert.That(view.Name, Is.EqualTo("myworkspace"));
        }

        [Test]
        public void when_mapping_Workspace_to_WorkspaceApiView_should_map_DisplayName()
        {
            var workspace = new Workspace("myworkspace", "My Workspace Display", DateTime.UtcNow);

            var view = mapper.Map<WorkspaceApiView>(workspace);

            Assert.That(view.DisplayName, Is.EqualTo("My Workspace Display"));
        }

        [Test]
        public void when_mapping_Workspace_to_WorkspaceApiView_should_map_CreatedAtUtc()
        {
            var createdAt = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
            var workspace = new Workspace("ws", "WS", createdAt);

            var view = mapper.Map<WorkspaceApiView>(workspace);

            Assert.That(view.CreatedAtUtc, Is.EqualTo(createdAt));
        }

        [Test]
        public void when_mapping_WorkspaceApiView_to_Workspace_should_map_Name()
        {
            var view = new WorkspaceApiView { Name = "reverse", DisplayName = "Reversed" };

            var workspace = mapper.Map<Workspace>(view);

            Assert.That(workspace.Name, Is.EqualTo("reverse"));
        }

        [Test]
        public void when_mapping_WorkspaceApiView_to_Workspace_should_map_DisplayName()
        {
            var view = new WorkspaceApiView { Name = "reverse", DisplayName = "Reversed Display" };

            var workspace = mapper.Map<Workspace>(view);

            Assert.That(workspace.DisplayName, Is.EqualTo("Reversed Display"));
        }

        [Test]
        public void when_roundtrip_mapping_Workspace_should_preserve_Name_and_DisplayName()
        {
            var original = new Workspace("roundtrip", "Round Trip Display", null);

            var view = mapper.Map<WorkspaceApiView>(original);
            var restored = mapper.Map<Workspace>(view);

            Assert.That(restored.Name, Is.EqualTo(original.Name));
            Assert.That(restored.DisplayName, Is.EqualTo(original.DisplayName));
        }

        [Test]
        public void when_mapping_Workspace_with_null_CreatedAt_should_map_null()
        {
            var workspace = new Workspace("ws", "WS", null);

            var view = mapper.Map<WorkspaceApiView>(workspace);

            Assert.That(view.CreatedAtUtc, Is.Null);
        }
    }
}

