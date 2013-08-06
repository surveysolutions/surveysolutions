using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping;
using NUnit.Framework;

namespace Ncqrs.Tests.Commanding.CommandExecution.Mapping
{
    [TestFixture]
    public class MappingForCommandNotFoundExceptionTests
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
	        var aCommand = new Mock<ICommand>();

	            var target = new MappingNotFoundException(message, aCommand.Object);

            target.Message.Should().Be(message);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_command()
        {
            String aMessage = "Hello world";
	        var theCommand = new Mock<ICommand>();

            var target = new MappingNotFoundException(aMessage, theCommand.Object);

	        Assert.True(target.Command == theCommand.Object);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_inner_exception()
        {
            String aMessage = "Hello world";
	        var theCommand = new Mock<ICommand>();
            var theInnerException = new Exception();

            var target = new MappingNotFoundException(aMessage, theCommand.Object, theInnerException);

            target.InnerException.Should().Be(theInnerException);
        }

        [Test]
        public void It_should_be_serializable()
        {
            var aMessage = "Hello world";
	        var aCommand = new Mock<ICommand>();
            var theException = new MappingNotFoundException(aMessage, aCommand.Object);
            MappingNotFoundException deserializedException = null;

            using (var buffer = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(buffer, theException);

                buffer.Seek(0, SeekOrigin.Begin);
                deserializedException = (MappingNotFoundException)formatter.Deserialize(buffer);
            }

            deserializedException.Should().NotBeNull();
        }
    }
}
