using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.PrefilledQuestionsViewModelTests
{
    internal class when_opening_not_existed_interview : PrefilledQuestionsViewModelTestContext
    {
        Establish context = () =>
        {
            loggerMock = new Mock<ILogger>();
            interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();
            navigationServiceMock = new Mock<IViewModelNavigationService>();

            viewModel = CreatePrefilledQuestionsViewModel(
                interviewRepository: interviewRepositoryMock.Object,
                viewModelNavigationService: navigationServiceMock.Object,
                logger: loggerMock.Object);
            viewModel.Init(notExistedInterviewId);
        };

        Because of = () => viewModel.Load();

        It should_check_interview_in_repository = () =>
            interviewRepositoryMock.Verify(ns => ns.Get(notExistedInterviewId), Times.Once());

        It should_logged_error = () =>
            loggerMock.Verify(ns => ns.Error(Moq.It.IsAny<string>(), null), Times.Once());

        It should_navigate_to_dashboard = () =>
            navigationServiceMock.Verify(ns => ns.NavigateToDashboard(null), Times.Once());


        private static string notExistedInterviewId = "same id";
        private static PrefilledQuestionsViewModel viewModel;
        private static Mock<ILogger> loggerMock;
        private static Mock<IStatefulInterviewRepository> interviewRepositoryMock;
        private static Mock<IViewModelNavigationService> navigationServiceMock;
    }
}