using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewPackagesServiceTests
{
    internal class when_sync_package_contain_zero_escape_characters
    {
        [Test]
        public void should_exclude_zero_escape_from_json()
        {
            SynchronizeInterviewEventsCommand syncCommand = null;

            Mock<ICommandService> commandService = new Mock<ICommandService>();
            commandService
                .Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Callback((ICommand c, string o) => { syncCommand = c as SynchronizeInterviewEventsCommand; });


            IComponentRegistration componentRegistration = new Mock<IComponentRegistration>().Object;
            var componentRegistry = new Mock<IComponentRegistry>();
            componentRegistry.Setup(x =>
                    x.TryGetRegistration(It.IsAny<Service>(), out componentRegistration))
                .Returns(true);

            var container = new Mock<ILifetimeScope>();
            container.Setup(x => x.BeginLifetimeScope(It.IsAny<string>())).Returns(container.Object);
            container.SetupGet(x => x.ComponentRegistry).Returns(componentRegistry.Object);


            var serviceLocatorNestedMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            serviceLocatorNestedMock.Setup(x => x.GetInstance<ICommandService>()).Returns(commandService.Object);
            serviceLocatorNestedMock.Setup(x => x.GetInstance<IJsonAllTypesSerializer>())
                .Returns(new JsonAllTypesSerializer());

            var packageStore = new Mock<IPlainStorageAccessor<ReceivedPackageLogEntry>>();
            packageStore.Setup(x => x.Query(It.IsAny<Func<IQueryable<ReceivedPackageLogEntry>, ReceivedPackageLogEntry>>())).Returns((ReceivedPackageLogEntry)null);

            serviceLocatorNestedMock.Setup(x => x.GetInstance<IPlainStorageAccessor<ReceivedPackageLogEntry>>()).Returns(packageStore.Object);
            serviceLocatorNestedMock.Setup(x => x.GetInstance<IInterviewUniqueKeyGenerator> ()).Returns(Mock.Of<IInterviewUniqueKeyGenerator>);

            container.Setup(x => x.ResolveComponent(It.IsAny<IComponentRegistration>(), It.IsAny<System.Collections.Generic.IEnumerable<Autofac.Core.Parameter>>()))
                .Returns((IComponentRegistration compRegistration, IEnumerable<Autofac.Core.Parameter> pars) =>
                    {
                        return serviceLocatorNestedMock.Object;
                    });

            var autofacServiceLocatorAdapterForTests = new AutofacServiceLocatorAdapter(container.Object);

            var serviceLocatorOriginal = ServiceLocator.IsLocationProviderSet ? ServiceLocator.Current : null;

            ServiceLocator.SetLocatorProvider(() => autofacServiceLocatorAdapterForTests);
            try
            {
                var service = Create.Service.InterviewPackagesService(commandService: commandService.Object);

                // Act
                service.ProcessPackage(Create.Entity.InterviewPackage(Guid.NewGuid(),
                    Create.Event.TextQuestionAnswered(answer: new string(new[] { 'a', '\0', '1' }))));
            }
            finally
            {
                ServiceLocator.SetLocatorProvider(() => serviceLocatorOriginal);
            }

            // Assert
            Assert.That(syncCommand, Is.Not.Null);
            Assert.That(syncCommand.SynchronizedEvents[0], Has.Property(nameof(TextQuestionAnswered.Answer)).EqualTo("a1"));
        }
        
        private static IServiceLocator serviceLocatorOriginal = null;
    }
}
