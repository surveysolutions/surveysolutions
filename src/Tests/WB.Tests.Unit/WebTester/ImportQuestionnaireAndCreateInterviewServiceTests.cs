using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Scenarios;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.WebTester;

[TestOf(typeof(ImportQuestionnaireAndCreateInterviewService))]
public class ImportQuestionnaireAndCreateInterviewServiceTests
{
    [Test]
    public void when_first_answer_cannot_be_applied_should_restore_interview_partially()
    {
        var originalInterviewId = Guid.NewGuid();
        var newInterviewId = Guid.NewGuid();
        var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
        var originalInterviewCommands = new List<InterviewCommand>
        {
            Create.Command.CreateInterview(interviewId: originalInterviewId),
            Create.Command.AnswerNumericIntegerQuestionCommand(interviewId: originalInterviewId)
        };
        var originalCommandsSnapshot = originalInterviewCommands.ToArray();
        var executedCommandsStorage = new Mock<ICacheStorage<List<InterviewCommand>, Guid>>();
        executedCommandsStorage
            .Setup(x => x.Get(originalInterviewId, originalInterviewId))
            .Returns(originalInterviewCommands);

        var scenarioService = new Mock<IScenarioService>();
        scenarioService
            .Setup(x => x.ConvertFromInterview(It.IsAny<IQuestionnaire>(), It.IsAny<IEnumerable<InterviewCommand>>()))
            .Returns(new List<IScenarioCommand> { new AnswerInteger("numeric", RosterVector.Empty, 1) });
        scenarioService
            .Setup(x => x.ConvertFromScenario(It.IsAny<IQuestionnaire>(), It.IsAny<IEnumerable<IScenarioCommand>>()))
            .Returns(new List<InterviewCommand> { Create.Command.AnswerNumericIntegerQuestionCommand() });

        var evictionService = new Mock<IEvictionNotifier>();
        var aggregateRootCache = new Mock<IAggregateRootCache>();
        var questionnaireImportService = new Mock<IQuestionnaireImportService>();
        var imageFileStorage = new Mock<IImageFileStorage>();
        imageFileStorage
            .Setup(x => x.GetBinaryFilesForInterview(originalInterviewId))
            .ReturnsAsync(new List<InterviewBinaryDataDescriptor>());
        questionnaireImportService
            .Setup(x => x.ImportQuestionnaire(It.IsAny<Guid>(), newInterviewId))
            .ReturnsAsync(questionnaireIdentity);
        var questionnaireStorage = new Mock<IQuestionnaireStorage>();
        questionnaireStorage
            .Setup(x => x.GetQuestionnaire(questionnaireIdentity, null))
            .Returns(Mock.Of<IQuestionnaire>());
        var statusStore = new Mock<IImportStatusStore>();
        statusStore.Setup(x => x.TryInitialize(newInterviewId)).Returns(true);
        var commandService = new Mock<ICommandService>();
        commandService
            .Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
            .Callback<ICommand, string>((command, _) =>
            {
                if (command is AnswerNumericIntegerQuestionCommand)
                    throw new AnswerNotAcceptedException("Incompatible answer");
            });
        var subject = new ImportQuestionnaireAndCreateInterviewService(
            executedCommandsStorage.Object,
            commandService.Object,
            imageFileStorage.Object,
            evictionService.Object,
            questionnaireImportService.Object,
            Mock.Of<IDesignerWebTesterApi>(),
            scenarioService.Object,
            questionnaireStorage.Object,
            Mock.Of<IScenarioSerializer>(),
            aggregateRootCache.Object,
            statusStore.Object,
            Mock.Of<ILogger<ImportQuestionnaireAndCreateInterviewService>>());

        subject.StartImportQuestionnaireAndCreateInterview(
            Guid.NewGuid(), newInterviewId, originalInterviewId, null);

        statusStore.Verify(x => x.Set(newInterviewId, CreationResult.DataPartialRestored), Times.Once);
        Assert.That(originalInterviewCommands, Is.EqualTo(originalCommandsSnapshot));
        evictionService.Verify(x => x.Evict(It.IsAny<Guid>()), Times.Never);
        aggregateRootCache.Verify(x => x.Evict(It.IsAny<Guid>()), Times.Never);
    }
}
