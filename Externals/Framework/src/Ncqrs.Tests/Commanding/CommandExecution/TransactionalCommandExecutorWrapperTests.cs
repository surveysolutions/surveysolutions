using System;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Commanding.CommandExecution;
using Rhino.Mocks;
using Ncqrs.Commanding;
using System.Transactions;
using NUnit.Framework;
using MockRepository = Rhino.Mocks.MockRepository;

namespace Ncqrs.Tests.Commanding.CommandExecution
{
    [TestFixture]
    public class TransactionalCommandExecutorWrapperTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        public class DummyCommand : CommandBase
        {
        }        

        [Test]
        public void When_executing_it_it_should_call_the_executor_given_via_ctor()
        {
            var theCommand = new DummyCommand();
            var theExecutor = MockRepository.GenerateMock<ICommandExecutor<DummyCommand>>();
            var theWrapper = new TransactionalCommandExecutorWrapper<DummyCommand>(theExecutor);

            theWrapper.Execute(theCommand);

            theExecutor.AssertWasCalled((e) => e.Execute(theCommand));
        }
    }
}
