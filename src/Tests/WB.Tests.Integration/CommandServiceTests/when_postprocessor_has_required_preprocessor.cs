using System;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Tests.Abc;

namespace WB.Tests.Integration.CommandServiceTests
{
    public class when_postprocessor_has_required_preprocessor
    {
        public class PlainCommandWithPostAndPreProcessor : ICommand { public Guid CommandIdentifier => Guid.Empty; }

        public class Aggregate : IPlainAggregateRoot
        {
            public void SetId(Guid id) { }
            public void Handle(PlainCommandWithPostAndPreProcessor commandWithPostAndPreProcessor) { }
        }

        [RequiresPreprocessor(typeof(PreProcessor))]
        public class PostProcessor : ICommandPostProcessor<Aggregate, PlainCommandWithPostAndPreProcessor>
        {
            public virtual void Process(Aggregate aggregate, PlainCommandWithPostAndPreProcessor commandWithPostAndPreProcessor)
            {
            }
        }

        public class PreProcessor : ICommandPreProcessor<Aggregate, PlainCommandWithPostAndPreProcessor>
        {
            public virtual void Process(Aggregate aggregate, PlainCommandWithPostAndPreProcessor command)
            {
            }
        }

        [Test]
        public void should_execute_preprocessor_and_postprocessor()
        {
            Aggregate aggregate = new Aggregate();

            CommandRegistry
                .Setup<Aggregate>()
                .InitializesWith<PlainCommandWithPostAndPreProcessor>(_ => Guid.Empty, agg => agg.Handle, config => config.PostProcessBy<PostProcessor>());

            var mockOfPostProcessor = new Mock<PostProcessor>();
            var mockOfPreProcessor = new Mock<PostProcessor>();

            var postProcessor = mockOfPostProcessor.Object;
            var processor = mockOfPreProcessor.Object;

            var serviceLocator = Mock.Of<IServiceLocator>(_
                => _.GetInstance(typeof(Aggregate)) == aggregate
                   && _.GetInstance(typeof(PostProcessor)) == postProcessor
                   && _.GetInstance(typeof(PreProcessor)) == processor);

            var commandService = Create.Service.CommandService(serviceLocator: serviceLocator);
            PlainCommandWithPostAndPreProcessor commandWithPostAndPreProcessor = new PlainCommandWithPostAndPreProcessor();

            // Act
            commandService.Execute(commandWithPostAndPreProcessor, null);

            // Assert
            mockOfPreProcessor.Verify(x => x.Process(aggregate, commandWithPostAndPreProcessor), Times.Once);
        }
    }
}
