using System;
using FluentAssertions;
using Moq;
using Ncqrs.Domain;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_command_handled_by_event_sourced_aggregate_and_validator_throws_exception
    {
        private class InvalidEventSourcedCommand : ICommand { public Guid CommandIdentifier => Guid.Empty; }

        private class Aggregate : EventSourcedAggregateRoot
        {
            public void Handle(InvalidEventSourcedCommand command) { }
        }

        private class Validator : ICommandValidator<Aggregate, InvalidEventSourcedCommand>
        {
            public void Validate(Aggregate aggregate, InvalidEventSourcedCommand command)
            {
                throw validatorException;
            }
        }

        [NUnit.Framework.OneTimeSetUp] public void context () {
            CommandRegistry
                .Setup<Aggregate>()
                .InitializesWith<InvalidEventSourcedCommand>(_ => Guid.Empty, aggregate => aggregate.Handle, config => config.ValidatedBy<Validator>());

            var serviceLocator = Mock.Of<IServiceLocator>(_
                => _.GetInstance(typeof(Aggregate)) == new Aggregate()
                && _.GetInstance(typeof(Validator)) == new Validator());

            commandService = Create.Service.CommandService(serviceLocator: serviceLocator);

            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Assert.Throws<Exception>(() => commandService.Execute(new InvalidEventSourcedCommand(), null));

        [NUnit.Framework.Test] public void should_rethrow_exception_thrown_by_aggregate () =>
            exception.Should().Be(validatorException);

        private static Exception exception;
        private static Exception validatorException = new Exception("Validation failed.");
        private static CommandService commandService;
    }
}
