using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Commanding.CommandExecution;
using NUnit.Framework;
using Ncqrs.Commanding;
using WB.Core.Infrastructure.CommandBus;

namespace Ncqrs.Tests.Commanding
{
    [TestFixture]
    public class ExecutorForCommandNotFoundExceptionTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_message()
        {
            String message = "Hello world";
            Type aInstanceType = typeof(String);

            var target = new ExecutorForCommandNotFoundException(aInstanceType, message);

            target.Message.Should().Be(message);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_instance_type()
        {
            String message = "Hello world";
            Type theCommandType = typeof(ICommand);

            var target = new ExecutorForCommandNotFoundException(theCommandType, message);

            target.CommandType.Should().Be(theCommandType);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_inner_exception()
        {
            String aMessage = "Hello world";
            Type aCommandType = typeof(ICommand);
            var theInnerException = new Exception();

            var target = new ExecutorForCommandNotFoundException(aCommandType, aMessage, theInnerException);

            target.InnerException.Should().Be(theInnerException);
        }
    }
}
