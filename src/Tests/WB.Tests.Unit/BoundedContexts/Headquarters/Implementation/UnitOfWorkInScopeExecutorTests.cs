using System.Threading.Tasks;
using Autofac;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Implementation
{
    [TestOf(typeof(UnitOfWorkInScopeExecutor))]
    public class UnitOfWorkInScopeExecutorTests
    {
        [Test]
        public void when_executing_in_scope_should_expose_and_restore_ambient_unit_of_work()
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            var ambientUnitOfWorkAccessor = new AmbientUnitOfWorkAccessor();
            using var container = CreateContainer(unitOfWork.Object, ambientUnitOfWorkAccessor);
            var executor = new UnitOfWorkInScopeExecutor(container, ambientUnitOfWorkAccessor);

            executor.Execute(locator =>
            {
                Assert.That(locator, Is.InstanceOf<IServiceLocator>());
                Assert.That(ambientUnitOfWorkAccessor.Current, Is.SameAs(unitOfWork.Object));
            });

            Assert.That(ambientUnitOfWorkAccessor.Current, Is.Null);
            unitOfWork.Verify(x => x.AcceptChanges(), Times.Once);
        }

        [Test]
        public async Task when_executing_async_in_scope_should_restore_ambient_unit_of_work_after_await()
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            var ambientUnitOfWorkAccessor = new AmbientUnitOfWorkAccessor();
            using var container = CreateContainer(unitOfWork.Object, ambientUnitOfWorkAccessor);
            var executor = new UnitOfWorkInScopeExecutor(container, ambientUnitOfWorkAccessor);

            await executor.ExecuteAsync(async locator =>
            {
                await Task.Yield();
                Assert.That(locator, Is.InstanceOf<IServiceLocator>());
                Assert.That(ambientUnitOfWorkAccessor.Current, Is.SameAs(unitOfWork.Object));
            });

            Assert.That(ambientUnitOfWorkAccessor.Current, Is.Null);
            unitOfWork.Verify(x => x.AcceptChanges(), Times.Once);
        }

        private static IContainer CreateContainer(IUnitOfWork unitOfWork, AmbientUnitOfWorkAccessor ambientUnitOfWorkAccessor)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(unitOfWork).As<IUnitOfWork>();
            builder.RegisterInstance(ambientUnitOfWorkAccessor);
            builder.RegisterInstance(Mock.Of<IWorkspaceContextAccessor>(x => x.CurrentWorkspace() == null));
            builder.RegisterType<AutofacServiceLocatorAdapter>().As<IServiceLocator>();
            return builder.Build();
        }
    }
}
