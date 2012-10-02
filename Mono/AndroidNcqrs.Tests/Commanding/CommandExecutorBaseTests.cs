using System;
using AndroidMocks;
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
            var factory = new DynamicMock<IUnitOfWorkFactory>();
			factory.Expect(f => f.CreateUnitOfWork(Guid.NewGuid()), null);

            var aCommand = new FooCommand()
                               {
                                   CommandIdentifier = Guid.NewGuid()
                               };
            var executor = new FooCommandExecutor(factory.Instance);
            executor.Execute(aCommand);

            factory.AssertWasCalled(f => f.CreateUnitOfWork(aCommand.CommandIdentifier));
        }

        [Test]
        public void Executing_should_call_ExecuteInContext_with_context_from_factory()
        {            
            var context = new DynamicMock<IUnitOfWorkContext>();
			context.Stub(c => c.Dispose());

            var factory = new DynamicMock<IUnitOfWorkFactory>();
            factory.Stub(f => f.CreateUnitOfWork(Guid.NewGuid()), context.Instance);

            var aCommand = new FooCommand();
            var executor = new FooCommandExecutor(factory.Instance);
            executor.Execute(aCommand);

            Assert.True(executor.LastGivenContext == context.Instance);
        }

        [Test]
        public void Executing_should_call_ExecuteInContext_with_given_command()
        {
			var context = new DynamicMock<IUnitOfWorkContext>();
			context.Stub(c => c.Dispose());

			var factory = new DynamicMock<IUnitOfWorkFactory>();
			factory.Stub(f => f.CreateUnitOfWork(Guid.NewGuid()), context.Instance);

			var theCommand = new FooCommand();
			var executor = new FooCommandExecutor(factory.Instance);
			executor.Execute(theCommand);

			executor.LastGivenCommand.Should().Be(theCommand);
        }
    }
}
