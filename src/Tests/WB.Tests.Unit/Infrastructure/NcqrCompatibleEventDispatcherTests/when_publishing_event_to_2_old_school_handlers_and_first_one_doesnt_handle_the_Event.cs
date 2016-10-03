using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_to_2_old_school_handlers_and_first_one_doesnt_handle_the_event : NcqrCompatibleEventDispatcherTestContext
    {
        Establish context = () =>
        {
            publishableEvent = Create.PublishedEvent.InterviewCreated();

            eventDispatcher = Create.Service.NcqrCompatibleEventDispatcher();

            eventDispatcher.TransactionManager = Mock.Of<ITransactionManagerProvider>(_=>_.GetTransactionManager()== transactionManagerMock.Object);

            eventDispatcher.Register(Mock.Of<IEventHandler>());

            var questionClonedHandlerMock = new Mock<IEventHandler>();
            questionClonedHandlerMock.As<IEventHandler<InterviewCreated>>();

            eventDispatcher.Register(questionClonedHandlerMock.Object);
        };

        Because of = () => eventDispatcher.Publish(publishableEvent.ToEnumerable().ToArray());

        It should_open_command_transaction_only_once = () =>
            transactionManagerMock.Verify(x=>x.BeginCommandTransaction(), Times.Once);

        It should_commit_command_transaction_only_once = () =>
            transactionManagerMock.Verify(x => x.CommitCommandTransaction(), Times.Once);

        private static NcqrCompatibleEventDispatcher eventDispatcher;
        private static IPublishableEvent publishableEvent;
        
        private static Mock<ITransactionManager> transactionManagerMock = new Mock<ITransactionManager>();
    }
}