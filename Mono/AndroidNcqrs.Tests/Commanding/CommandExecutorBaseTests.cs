using System;
using Microsoft.Practices.ServiceLocation;
using Moq;
using FluentAssertions;
using Ncqrs.Commanding.CommandExecution;
using Ncqrs.Domain;
using NUnit.Framework;
using Ncqrs.Commanding;

namespace Ncqrs.Tests.Commanding
{
    [TestFixture]
    public class CommandExecutorBaseTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        public class FooCommand : ICommand
        {
            public Guid CommandIdentifier { get; set; }

            public long? KnownVersion { get; set; }

            public FooCommand()
            {
                CommandIdentifier = Guid.NewGuid();
            }
        }

        public class FooCommandExecutor : CommandExecutorBase<FooCommand>
        {
            public IUnitOfWorkContext LastGivenContext { get; private set;}

            public FooCommand LastGivenCommand { get; private set; }

            public FooCommandExecutor()
            {
            }

            public FooCommandExecutor(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
            {
            }

            protected override void ExecuteInContext(IUnitOfWorkContext context, FooCommand command)
            {
                LastGivenContext = context;
                LastGivenCommand = command;
            }
        }

        [Test]
        public void Executing_one_with_a_custom_factory_should_give_context_created_with_that_factory()
        {
	        var factory = new Mock<IUnitOfWorkFactory>();
	        factory.Setup(f => f.CreateUnitOfWork(Guid.NewGuid()));

            var aCommand = new FooCommand()
                               {
                                   CommandIdentifier = Guid.NewGuid()
                               };
            var executor = new FooCommandExecutor(factory.Object);
            executor.Execute(aCommand);

            factory.Verify(f => f.CreateUnitOfWork(It.IsAny<Guid>()));
        }

        [Test]
        public void Executing_should_call_ExecuteInContext_with_context_from_factory()
        {            
            var context = new Mock<IUnitOfWorkContext>();

            var factory = new Mock<IUnitOfWorkFactory>();
			factory.Setup(f => f.CreateUnitOfWork(It.IsAny<Guid>())).Returns(context.Object);

            var aCommand = new FooCommand();
            var executor = new FooCommandExecutor(factory.Object);
            executor.Execute(aCommand);

            Assert.True(executor.LastGivenContext == context.Object);
        }

        [Test]
        public void Executing_should_call_ExecuteInContext_with_given_command()
        {
			var context = new Mock<IUnitOfWorkContext>();

			var factory = new Mock<IUnitOfWorkFactory>();
	        factory.Setup(f => f.CreateUnitOfWork(Guid.NewGuid())).Returns(context.Object);

			var theCommand = new FooCommand();
			var executor = new FooCommandExecutor(factory.Object);
			executor.Execute(theCommand);

			executor.LastGivenCommand.Should().Be(theCommand);
        }
    }
}
