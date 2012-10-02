using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AndroidMocks;
using FluentAssertions;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping;
using NUnit.Framework;

namespace Ncqrs.Tests.Commanding.CommandExecution.Mapping
{
    [TestFixture]
    public class MappingForCommandNotFoundExceptionTests
    {
        [Test]
        public void Constructing_an_instance_should_initialize_the_message()
        {
            String message = "Hello world";
            var aCommand = new DynamicMock<ICommand>();

            var target = new MappingNotFoundException(message, aCommand.Instance);

            target.Message.Should().Be(message);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_command()
        {
            String aMessage = "Hello world";
            var theCommand = new DynamicMock<ICommand>();

            var target = new MappingNotFoundException(aMessage, theCommand.Instance);

	        Assert.True(target.Command == theCommand.Instance);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_inner_exception()
        {
            String aMessage = "Hello world";
            var theCommand = new DynamicMock<ICommand>();
            var theInnerException = new Exception();

            var target = new MappingNotFoundException(aMessage, theCommand.Instance, theInnerException);

            target.InnerException.Should().Be(theInnerException);
        }

        [Test]
        public void It_should_be_serializable()
        {
            var aMessage = "Hello world";
            var aCommand = new DynamicMock<ICommand>();
            var theException = new MappingNotFoundException(aMessage, aCommand.Instance);
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
