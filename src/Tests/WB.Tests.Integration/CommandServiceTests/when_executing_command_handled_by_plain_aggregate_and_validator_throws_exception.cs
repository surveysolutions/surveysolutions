using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_command_handled_by_plain_aggregate_and_validator_throws_exception
    {
        private class InvalidPlainCommand : ICommand { public Guid CommandIdentifier => Guid.Empty; }

        private class Aggregate : IPlainAggregateRoot
        {
            public void SetId(Guid id) { }
            public void Handle(InvalidPlainCommand command) { }
        }

        private class Validator : ICommandValidator<Aggregate, InvalidPlainCommand>
        {
            public void Validate(Aggregate aggregate, InvalidPlainCommand command)
            {
                throw validatorException;
            }
        }

        [NUnit.Framework.OneTimeSetUp] public void context () {
            CommandRegistry
                .Setup<Aggregate>()
                .InitializesWith<InvalidPlainCommand>(_ => Guid.Empty, aggregate => aggregate.Handle, config => config.ValidatedBy<Validator>());

            var serviceLocator = Mock.Of<IServiceLocator>(_
                => _.GetInstance(typeof(Aggregate)) == new Aggregate()
                && _.GetInstance(typeof(Validator)) == new Validator());

            commandService = Create.Service.CommandService(serviceLocator: serviceLocator);

            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Assert.Throws<Exception>(() => commandService.Execute(new InvalidPlainCommand(), null));

        [NUnit.Framework.Test] public void should_rethrow_exception_thrown_by_aggregate () =>
            exception.Should().Be(validatorException);

        private static Exception exception;
        private static Exception validatorException = new Exception("Validation failed.");
        private static CommandService commandService;
    }
}
