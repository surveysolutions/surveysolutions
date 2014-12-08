using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Logging;

namespace Ncqrs.Tests.Eventing.Storage
{
    [TestFixture]
    public class SimpleEventTypeResolverTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        private SimpleEventTypeResolver resolver = new SimpleEventTypeResolver();

        [Test]
        public void Resolves_types_to_event_names()
        {
            var type = typeof(ILogger);
            var result = resolver.EventNameFor(type);
            result.Should().Be(type.AssemblyQualifiedName);
        }

        [Test]
        public void Resolves_event_names_to_types()
        {
            var type = typeof(ILogger);
            var result = resolver.ResolveType(type.AssemblyQualifiedName);
            result.Should().Be(type);
        }
    }
}
