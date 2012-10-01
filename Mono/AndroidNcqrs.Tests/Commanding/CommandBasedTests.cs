using System;
using AndroidMocks;
using FluentAssertions;
using Ncqrs.Commanding;
using NUnit.Framework;

namespace Ncqrs.Tests.Commanding
{
    [TestFixture]
    public class CommandBasedTests
    {
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
            var generator = new DynamicMock<IUniqueIdentifierGenerator>();
            generator.Expect(t => t.GenerateNewId(), generatedId);

            NcqrsEnvironment.SetDefault<IUniqueIdentifierGenerator>(generator.Instance);

            var command = new FooCommand();

            generator.VerifyAllExpectations();
            command.CommandIdentifier.Should().Be(generatedId);

            NcqrsEnvironment.Deconfigure();
        }

        [Test]
        public void Constructing_with_custom_generator_should_it_to_generate_id()
        {
            var identifier = Guid.NewGuid();
            var generator = new DynamicMock<IUniqueIdentifierGenerator>();
            generator.Expect(t => t.GenerateNewId(), identifier);

            var command = new FooCommand(generator.Instance);

            generator.VerifyAllExpectations();
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
