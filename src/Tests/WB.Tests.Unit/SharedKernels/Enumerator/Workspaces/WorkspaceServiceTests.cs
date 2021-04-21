using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Workspaces
{
    [TestFixture]
    public class WorkspaceServiceTests
    {
        [Test]
        public void when_add_new_workspaces_should_save_it()
        {
            var workspaceRepository = Create.Storage.SqliteInmemoryStorage<WorkspaceView>();
            workspaceRepository.Store(new []
            {
                new WorkspaceView() { Id = "first" },
            });
            
            var workspaceService = CreateService(workspaceRepository);

            workspaceService.Save(new []
            {
                new WorkspaceView() { Id = "first" },
                new WorkspaceView() { Id = "second" },
                new WorkspaceView() { Id = "3" },
            });

            var storedWorkspaces = workspaceRepository.LoadAll();
            Assert.That(storedWorkspaces.Count, Is.EqualTo(3));
            Assert.That(storedWorkspaces.First().Id, Is.EqualTo("first"));
            Assert.That(storedWorkspaces.Second().Id, Is.EqualTo("second"));
            Assert.That(storedWorkspaces.Last().Id, Is.EqualTo("3"));
        }

        [Test]
        public void when_removed_one_workspace_on_server_should_delete_localy()
        {
            var workspaceRepository = Create.Storage.SqliteInmemoryStorage<WorkspaceView>();
            workspaceRepository.Store(new []
            {
                new WorkspaceView() { Id = "first" },
                new WorkspaceView() { Id = "second" },
            });
           
            var workspaceService = CreateService(workspaceRepository);

            workspaceService.Save(new []
            {
                new WorkspaceView() { Id = "first" },
            });

            var storedWorkspaces = workspaceRepository.LoadAll();
            Assert.That(storedWorkspaces.Count, Is.EqualTo(1));
            Assert.That(storedWorkspaces.First().Id, Is.EqualTo("first"));
        }
        
        [Test]
        public void when_on_server_add_new_workspace_and_removed_one_of_old_should_save_correct_workspaces_localy()
        {
            var workspaceRepository = Create.Storage.SqliteInmemoryStorage<WorkspaceView>();
            workspaceRepository.Store(new []
            {
                new WorkspaceView() { Id = "first" },
                new WorkspaceView() { Id = "second" },
            });
            
            var workspaceService = CreateService(workspaceRepository);

            workspaceService.Save(new []
            {
                new WorkspaceView() { Id = "first" },
                new WorkspaceView() { Id = "five" },
            });

            var storedWorkspaces = workspaceRepository.LoadAll();
            Assert.That(storedWorkspaces.Count, Is.EqualTo(2));
            Assert.That(storedWorkspaces.First().Id, Is.EqualTo("first"));
            Assert.That(storedWorkspaces.Second().Id, Is.EqualTo("five"));
        }

        private IWorkspaceService CreateService(
            IPlainStorage<WorkspaceView> workspaceRepository = null,
            SqliteSettings settings = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IInScopeExecutor executeInWorkspaceService = null)
        {
            return new WorkspaceService(
                workspaceRepository ?? Mock.Of<IPlainStorage<WorkspaceView>>(),
                settings ?? new SqliteSettings(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(s => s.IsDirectoryExists(It.IsAny<string>()) == true),
                executeInWorkspaceService ?? new TestInScopeExecutor(),
                Mock.Of<ILogger>());
        }
        
        private class TestInScopeExecutor : IInScopeExecutor
        {
            public void Execute(Action<IServiceLocator> action, string workspace = null)
            {
                action.Invoke(Mock.Of<IServiceLocator>(s =>
                    s.GetInstance<IPlainStorage<InterviewView>>() == Mock.Of<IPlainStorage<InterviewView>>()));
            }

            public T Execute<T>(Func<IServiceLocator, T> func, string workspace = null)
            {
                throw new NotImplementedException();
            }

            public Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func, string workspace = null)
            {
                throw new NotImplementedException();
            }

            public Task ExecuteAsync(Func<IServiceLocator, Task> func, string workspace = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}