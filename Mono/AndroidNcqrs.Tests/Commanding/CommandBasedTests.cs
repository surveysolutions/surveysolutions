using System;
using Microsoft.Practices.ServiceLocation;
using Moq;
using FluentAssertions;
using Ncqrs.Commanding;
using NUnit.Framework;

namespace Ncqrs.Tests.Commanding
{
    [TestFixture]
    public class CommandBasedTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }


        public class FooCommand : CommandBase
        {
            public FooCommand(Guid commandIdentifier) : base(commandIdentifier)
            {
            }

            public FooCommand()
            {
            }

            public FooCommand(IUniqueIdentifierGenerator idGenerator)
                : base(idGenerator)
            {
            }
        }

        [Test]
        public void Constructing_without_any_parameters_should_use_IUniqueIdentifierGenerator_to_generate_id()
        {
            var generatedId = Guid.NewGuid();
	        var generator = new Mock<IUniqueIdentifierGenerator>();
            generator.Setup(t => t.GenerateNewId()).Returns(generatedId);

            NcqrsEnvironment.SetDefault(generator.Object);

            var command = new FooCommand();

            generator.VerifyAll();
            command.CommandIdentifier.Should().Be(generatedId);

            NcqrsEnvironment.Deconfigure();
        }

        [Test]
        public void Constructing_with_custom_generator_should_it_to_generate_id()
        {
            var identifier = Guid.NewGuid();
	        var generator = new Mock<IUniqueIdentifierGenerator>();
            generator.Setup(t => t.GenerateNewId()).Returns(identifier);

            var command = new FooCommand(generator.Object);

            generator.VerifyAll();
            command.CommandIdentifier.Should().Be(identifier);
        }

        [Test]
        public void Constructing_with_a_direct_id_should_set_the_given_value()
        {
            var identifier = Guid.NewGuid();
            var command = new FooCommand(identifier);
            command.CommandIdentifier.Should().Be(identifier);
        }
    }
}
