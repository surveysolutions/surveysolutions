using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireDownloadViewModel))]
    public class QuestionnaireReloadTests : MvxIoCSupportingTest
    {
        [SetUp]
        public void SetUp() => Setup();

        [Test]
        public async Task when_reloading_during_an_answer_should_replay_a_snapshot_with_a_new_questionnaire_identity()
        {
            var questionnaireId = Guid.NewGuid();
            var interviewId = Guid.NewGuid();
            var questionnaire = new Questionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(), "assembly");
            var designer = new Mock<IDesignerApiService>();
            designer.Setup(x => x.GetQuestionnaireAsync(It.IsAny<string>(), It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(questionnaire);
            designer.Setup(x => x.GetTranslationsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<TranslationDto>());
            designer.Setup(x => x.GetReusableCategoriesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<ReusableCategoriesDto>());

            var pendingAnswer = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var waiting = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var replayed = new List<InterviewCommand>();
            var commands = new Mock<ICommandService>();
            commands.Setup(x => x.WaitPendingCommandsAsync())
                .Callback(() => waiting.SetResult(true))
                .Returns(pendingAnswer.Task);
            commands.Setup(x => x.ExecuteAsync(It.IsAny<ICommand>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<ICommand, string, CancellationToken>((command, _, _) => replayed.Add((InterviewCommand)command))
                .Returns(Task.CompletedTask);
            var navigation = new Mock<IViewModelNavigationService>();
            navigation.Setup(x => x.NavigateToInterviewAsync(It.IsAny<string>(), It.IsAny<NavigationIdentity>()))
                .ReturnsAsync(true);
            var attachments = new Mock<IAttachmentContentStorage>();
            var storage = new ExecutedCommandsStorage();
            var downloader = new QuestionnaireDownloadViewModel(Mock.Of<ITesterPrincipal>(), designer.Object,
                commands.Object, Mock.Of<IQuestionnaireImportService>(), navigation.Object,
                Mock.Of<IFriendlyErrorMessageService>(), Mock.Of<IUserInteractionService>(), Mock.Of<ILogger>(),
                storage, attachments.Object, Mock.Of<IQuestionnaireStorage>());
            var progress = new Progress<string>();
            var oldIdentity = await downloader.DownloadQuestionnaireWithAllDependenciesAsync(
                questionnaireId.ToString(), "test", progress, CancellationToken.None);
            var create = Create.Command.CreateInterview(interviewId: interviewId, questionnaireIdentity: oldIdentity);
            storage.Add(interviewId, create);

            var reload = downloader.ReloadQuestionnaireAsync(questionnaireId.ToString(), "test", interviewId,
                new NavigationIdentity(), progress, CancellationToken.None);
            await waiting.Task;
            var answer = Create.Command.AnswerNumericIntegerQuestionCommand(interviewId: interviewId, answer: 42);
            storage.Add(interviewId, answer);
            pendingAnswer.SetResult(true);

            (await reload).Should().BeTrue();
            replayed.Should().HaveCount(2);
            var newCreate = replayed.OfType<CreateInterview>().Single();
            newCreate.QuestionnaireId.Should().NotBe(oldIdentity);
            newCreate.QuestionnaireId.QuestionnaireId.Should().Be(questionnaireId);
            newCreate.InterviewId.Should().NotBe(interviewId);
            replayed[1].InterviewId.Should().Be(newCreate.InterviewId);
            create.InterviewId.Should().Be(interviewId);
            create.QuestionnaireId.Should().Be(oldIdentity);
            answer.InterviewId.Should().Be(interviewId);
            storage.Get(interviewId).Should().Equal(create, answer);
            attachments.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
