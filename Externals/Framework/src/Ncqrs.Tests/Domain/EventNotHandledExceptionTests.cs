using System;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Config;
using Ncqrs.Domain;
using NUnit.Framework;
using Ncqrs.Eventing;

namespace Ncqrs.Tests.Domain
{
    [TestFixture]
    public class EventNotHandledExceptionTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        public class FooEvent : Event
        {}

        [Test]
        public void Constructing_an_instance_should_initialize_the_message()
        {
            String message = "Hello world";
            IEvent aEvent = new FooEvent();

            var target = new EventNotHandledException(aEvent, message);

            target.Message.Should().Be(message);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_event()
        {
            String aMessage = "Hello world";
            IEvent theEvent = new FooEvent();

            var target = new EventNotHandledException(theEvent, aMessage);

            target.Event.Should().Be(theEvent);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_inner_exception()
        {
            String aMessage = "Hello world";
            IEvent aEvent = new FooEvent();
            var theInnerException = new Exception();

            var target = new EventNotHandledException(aEvent, aMessage, theInnerException);

            target.InnerException.Should().Be(theInnerException);
        }
    }
}
