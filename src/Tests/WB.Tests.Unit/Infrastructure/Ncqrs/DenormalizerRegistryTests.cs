using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.Ncqrs
{
    public class DenormalizerRegistryTests
    {
        class RegularDenormalizer : BaseDenormalizer,
            IEventHandler<InterviewCreated>
        {
            public void Handle(IPublishedEvent<InterviewCreated> evnt)
            {
            }
        }

        class FunctionalDenormalizer : IFunctionalEventHandler
        {
            public void Handle(IEnumerable<IPublishableEvent> publishableEvents)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void should_register_functional_denormalizer()
        {
            var registry = Create.Service.DenormalizerRegistryNative();
            
            // Act
            registry.RegisterFunctional<FunctionalDenormalizer>();

            // Assert
            Assert.That(registry.FunctionalDenormalizers, Has.Count.EqualTo(1));
            Assert.That(registry.FunctionalDenormalizers.First(), Is.EqualTo(typeof(FunctionalDenormalizer)));
        }

        [Test]
        public void should_register_regular_denormalizer()
        {
            var registry = Create.Service.DenormalizerRegistryNative();
            
            // Act
            registry.Register<RegularDenormalizer>();

            // Assert
            Assert.That(registry.SequentialDenormalizers, Has.Count.EqualTo(1));
            Assert.That(registry.SequentialDenormalizers.First(), Is.EqualTo(typeof(RegularDenormalizer)));
            Assert.That(registry.CanHandleEvent(typeof(RegularDenormalizer), Create.PublishedEvent.InterviewCreated()), Is.True);
        }

        
    }
}
