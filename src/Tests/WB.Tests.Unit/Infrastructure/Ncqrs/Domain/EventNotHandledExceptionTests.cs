using System;
using FluentAssertions;
using Moq;
using Ncqrs.Domain;
using NUnit.Framework;
using Ncqrs.Eventing;
using WB.Tests.Unit;

namespace Ncqrs.Tests.Domain
{
    [TestFixture]
    internal class EventNotHandledExceptionTests
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        public class FooEvent : IEvent
        {
            public Guid EventIdentifier { get; }
            public DateTime EventTimeStamp { get; }
        }

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
