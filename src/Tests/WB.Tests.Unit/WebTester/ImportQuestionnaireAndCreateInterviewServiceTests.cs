using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Scenarios;
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
        var executedCommandsStorage = new Mock<ICacheStorage<List<InterviewCommand>, Guid>>();
        executedCommandsStorage
            .Setup(x => x.Get(originalInterviewId, originalInterviewId))
            .Returns(new List<InterviewCommand> { Create.Command.AnswerNumericIntegerQuestionCommand() });

        var scenarioService = new Mock<IScenarioService>();
        scenarioService
            .Setup(x => x.ConvertFromInterview(It.IsAny<IQuestionnaire>(), It.IsAny<IEnumerable<InterviewCommand>>()))
            .Throws(new InterviewException("Incompatible answer"));

        var evictionService = new Mock<IEvictionNotifier>();
        var aggregateRootCache = new Mock<IAggregateRootCache>();
        var questionnaireImportService = new Mock<IQuestionnaireImportService>();
        questionnaireImportService
            .Setup(x => x.ImportQuestionnaire(It.IsAny<Guid>(), newInterviewId))
            .ReturnsAsync(questionnaireIdentity);
        var questionnaireStorage = new Mock<IQuestionnaireStorage>();
        questionnaireStorage
            .Setup(x => x.GetQuestionnaire(questionnaireIdentity, null))
            .Returns(Mock.Of<IQuestionnaire>());
        var statusStore = new Mock<IImportStatusStore>();
        statusStore.Setup(x => x.TryInitialize(newInterviewId)).Returns(true);
        var subject = new ImportQuestionnaireAndCreateInterviewService(
            executedCommandsStorage.Object,
            Mock.Of<ICommandService>(),
            Mock.Of<IImageFileStorage>(),
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
        evictionService.Verify(x => x.Evict(It.IsAny<Guid>()), Times.Never);
        aggregateRootCache.Verify(x => x.Evict(It.IsAny<Guid>()), Times.Never);
    }
}
