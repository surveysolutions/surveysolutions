using System;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Domain.Storage;
using NUnit.Framework;
using WB.Tests.Unit;

namespace Ncqrs.Tests.Domain.Storage
{
    [TestFixture]
    public class AggregateRootCreationExceptionTests
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_message()
        {
            String message = "Hello world";

            var target = new AggregateRootCreationException(message);

            target.Message.Should().Be(message);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_inner_exception()
        {
            String aMessage = "Hello world";
            var theInnerException = new Exception();

            var target = new AggregateRootCreationException(aMessage, theInnerException);

            target.InnerException.Should().Be(theInnerException);
        }
    }
}
