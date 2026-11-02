using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Moq;
using MvvmCross.Base;
using MvvmCross.Views;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.EnablementViewModelTests
{
    [TestFixture]
    internal class when_interview_is_null_in_get_enablement : EnablementViewModelTestsContext
    {
        [Test]
        public void should_return_false_and_not_navigate_to_dashboard()
        {
            base.Setup();

            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);

            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[] { Create.Entity.NumericIntegerQuestion(id: questionId) });
            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var questionIdentity = Create.Entity.Identity(questionId, RosterVector.Empty);

            // First two calls (Init + initial update) return the real interview; explicit call under test returns null
            var interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();
            interviewRepositoryMock.SetupSequence(r => r.Get(It.IsAny<string>()))
                .Returns(interview)
                .Returns(interview)
                .Returns(default(IStatefulInterview));

            var navigationServiceMock = new Mock<IViewModelNavigationService>();

            var viewModel = CreateViewModel(
                questionnaireRepository: questionnaireRepository,
                interviewRepository: interviewRepositoryMock.Object,
                viewModelNavigationService: navigationServiceMock.Object);

            viewModel.Init(interview.EventSourceId.FormatGuid(), questionIdentity);

            // Act: second repository call returns null (simulates interview evicted during reload)
            var result = viewModel.GetEnablementFromInterview();

            // Assert
            result.Should().BeFalse("a null interview is a transient state during reload and should disable the entity");
            navigationServiceMock.Verify(ns => ns.NavigateToDashboardAsync(It.IsAny<string>()), Times.Never,
                "navigation should not be triggered by GetEnablementFromInterview when interview is null");
        }
    }
}
