using System;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.CommandServiceTests
{
    public class when_executing_command_handled_by_plain_aggregate_with_post_processor
    {
        public class PlainCommandWithPostProcesor : ICommand { public Guid CommandIdentifier => Guid.Empty; }

        public class Aggregate : IPlainAggregateRoot
        {
            public void SetId(Guid id) { }
            public void Handle(PlainCommandWithPostProcesor commandWithPostProcesor) { }
        }

        public class PostProcessor : ICommandPostProcessor<Aggregate, PlainCommandWithPostProcesor>
        {
            public virtual void Process(Aggregate aggregate, PlainCommandWithPostProcesor commandWithPostProcesor)
            {
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .InitializesWith<PlainCommandWithPostProcesor>(_ => Guid.Empty, aggregate => aggregate.Handle, config => config.PostProcessBy<PostProcessor>());

            var serviceLocator = Mock.Of<IServiceLocator>(_
                => _.GetInstance(typeof(Aggregate)) == aggregate
                && _.GetInstance(typeof(PostProcessor)) == mockOfPostProcessor.Object);

            commandService = Create.CommandService(serviceLocator: serviceLocator);
        };

        Because of = () => commandService.Execute(commandWithPostProcesor, null);

        It should_call_process_method_in_command_post_processor = () =>
            mockOfPostProcessor.Verify(x=>x.Process(aggregate, commandWithPostProcesor), Times.Once);
        
        private static readonly Mock<PostProcessor> mockOfPostProcessor = new Mock<PostProcessor>();
        private static CommandService commandService;
        private static readonly Aggregate aggregate = new Aggregate();
        private static readonly PlainCommandWithPostProcesor commandWithPostProcesor = new PlainCommandWithPostProcesor();
    }
}