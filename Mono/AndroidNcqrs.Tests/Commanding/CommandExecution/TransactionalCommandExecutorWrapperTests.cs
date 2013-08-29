using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding.CommandExecution;
using Ncqrs.Commanding;
using NUnit.Framework;
using Moq;

namespace Ncqrs.Tests.Commanding.CommandExecution
{
    [TestFixture]
    public class TransactionalCommandExecutorWrapperTests
    {
        public class DummyCommand : CommandBase
        {
        }

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void When_executing_it_it_should_call_the_executor_given_via_ctor()
        {
            var theCommand = new DummyCommand();
            //var theExecutor = new DynamicMock<ICommandExecutor<DummyCommand>>();
	        var theExecutor = new Mock<ICommandExecutor<DummyCommand>>();
			theExecutor.Expect(e => e.Execute(null));

            var theWrapper = new TransactionalCommandExecutorWrapper<DummyCommand>(theExecutor.Object);

            theWrapper.Execute(theCommand);

            theExecutor.Verify(x => x.Execute(It.IsAny<DummyCommand>()));
        }
    }
}
