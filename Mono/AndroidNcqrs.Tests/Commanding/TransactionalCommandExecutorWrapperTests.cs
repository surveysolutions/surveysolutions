using System;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution;
using NUnit.Framework;

namespace Ncqrs.Tests.Commanding
{
    [TestFixture]
    public class TransactionalCommandExecutorWrapperTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void Executing_a_command_with_it_should_call_the_executor_that_was_set_at_construct()
        {
            var aExecutor = new Mock<ICommandExecutor<ICommand>>();
			aExecutor.Setup(e => e.Execute(It.IsAny<ICommand>()));

            var aCommand = new Mock<ICommand>();
            var theWrapper = new TransactionalCommandExecutorWrapper<ICommand>(aExecutor.Object);

            theWrapper.Execute(aCommand.Object);

            aExecutor.VerifyAll();
        }
    }
}
