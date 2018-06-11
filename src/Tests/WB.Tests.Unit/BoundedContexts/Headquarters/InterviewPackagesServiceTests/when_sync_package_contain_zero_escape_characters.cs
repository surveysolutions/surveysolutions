using System;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewPackagesServiceTests
{
    internal class when_sync_package_contain_zero_escape_characters
    {
        [Test]
        public void should_exclude_zero_escape_from_json()
        {
            SynchronizeInterviewEventsCommand syncCommand = null;
            Mock<ICommandService> commandService = new Mock<ICommandService>();
            commandService
                .Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Callback((ICommand c, string o) => { syncCommand = c as SynchronizeInterviewEventsCommand; });

            var service = Create.Service.InterviewPackagesService(commandService: commandService.Object);

            // Act
            service.ProcessPackage(Create.Entity.InterviewPackage(Guid.NewGuid(), Create.Event.TextQuestionAnswered(answer: new string(new []{'a', '\0', '1'}))));

            // Assert
            Assert.That(syncCommand, Is.Not.Null);
            Assert.That(syncCommand.SynchronizedEvents[0], Has.Property(nameof(TextQuestionAnswered.Answer)).EqualTo("a1"));
        }
    }
}