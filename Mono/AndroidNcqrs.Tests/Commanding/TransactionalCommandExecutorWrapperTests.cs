using System;
using AndroidMocks;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution;
using NUnit.Framework;

namespace Ncqrs.Tests.Commanding
{
    [TestFixture]
    public class TransactionalCommandExecutorWrapperTests
    {
        [Test]
        public void Executing_a_command_with_it_should_call_the_executor_that_was_set_at_construct()
        {
            var aExecutor = new DynamicMock<ICommandExecutor<ICommand>>();
			aExecutor.Expect(e => e.Execute(null));

            var aCommand = new DynamicMock<ICommand>();
            var theWrapper = new TransactionalCommandExecutorWrapper<ICommand>(aExecutor.Instance);

            theWrapper.Execute(aCommand.Instance);

            aExecutor.AssertWasCalled(e=>e.Execute(null));
        }
    }
}
