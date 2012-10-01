using AndroidMocks;
using Ncqrs.Commanding.CommandExecution;
using Ncqrs.Commanding;
using NUnit.Framework;

namespace Ncqrs.Tests.Commanding.CommandExecution
{
    [TestFixture]
    public class TransactionalCommandExecutorWrapperTests
    {
        public class DummyCommand : CommandBase
        {
        }        

        [Test]
        public void When_executing_it_it_should_call_the_executor_given_via_ctor()
        {
            var theCommand = new DummyCommand();
            var theExecutor = new DynamicMock<ICommandExecutor<DummyCommand>>();
			theExecutor.Stub(e => e.Execute(null));

            var theWrapper = new TransactionalCommandExecutorWrapper<DummyCommand>(theExecutor.Instance);

            theWrapper.Execute(theCommand);

            theExecutor.AssertWasCalled(e => e.Execute(null));
        }
    }
}
