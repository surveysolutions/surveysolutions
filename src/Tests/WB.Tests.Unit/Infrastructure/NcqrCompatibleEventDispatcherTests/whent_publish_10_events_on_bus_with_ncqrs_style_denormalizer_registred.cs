using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publish_10_events_on_bus_with_ncqrs_style_denormalizer_registered : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            ncqrsStyleEventHandlerMock = new Mock<TestDenormalzier>();

            var denormalizerRegistry = Create.Service.DenormalizerRegistryNative();
            denormalizerRegistry.Register<TestDenormalzier>();
            var serviceLocator = Mock.Of<IServiceLocator>(x =>
                x.GetInstance(typeof(TestDenormalzier)) == ncqrsStyleEventHandlerMock.Object);

            ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher(serviceLocator: serviceLocator, denormalizerRegistry: denormalizerRegistry);
            BecauseOf();
        }

        public void BecauseOf() => ncqrCompatibleEventDispatcher.Publish(CreatePublishableEvents(10));

        [NUnit.Framework.Test]
        public void should_regitred_denormalizer_method_handle_be_called_10_times() =>
            ncqrsStyleEventHandlerMock.Verify(x => x.Handle(Moq.It.IsAny<IPublishedEvent<InterviewCreated>>()), Times.Exactly(10));

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static Mock<TestDenormalzier> ncqrsStyleEventHandlerMock;
    }
}
