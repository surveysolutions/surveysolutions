using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Workspaces
{
    [TestFixture]
    public class WorkspacesServiceTests 
    {
        [Test]
        public void WorkspacesService_Should_return_only_enabled_services()
        {
            var enabledWorkspace = Create.Entity.Workspace();
            var disabledWorkspace = Create.Entity.Workspace();
            disabledWorkspace.Disable();
            
            var storage =  new TestPlainStorage<Workspace>();
            storage.Store(new []{enabledWorkspace, disabledWorkspace});

            // Act
            var service = Create.Service.WorkspacesService(storage);

            // Assert
            var list = service.GetEnabledWorkspaces().ToList();
            Assert.That(list, Has.Count.EqualTo(1));
            Assert.That(list.First().Name, Is.EqualTo(enabledWorkspace.Name));
        }
    }
}
