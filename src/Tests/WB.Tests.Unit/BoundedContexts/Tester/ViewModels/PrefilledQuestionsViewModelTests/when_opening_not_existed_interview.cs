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
            logger = Mock.Of<ILogger>();
            interviewRepository = Mock.Of<IStatefulInterviewRepository>();
            navigationService = Mock.Of<IViewModelNavigationService>();

            viewModel = CreatePrefilledQuestionsViewModel(interviewRepository: interviewRepository,
                viewModelNavigationService: navigationService,
                logger: logger);
        };

        Because of = () => viewModel.Init(notExistedInterviewId);

        It should_check_interview_in_repository = () =>
            Mock.Get(interviewRepository).Verify(ns => ns.Get(notExistedInterviewId), Times.Once());

        It should_logged_error = () =>
            Mock.Get(logger).Verify(ns => ns.Error(Moq.It.IsAny<string>(), null), Times.Once());

        It should_navigate_to_dashboard = () =>
            Mock.Get(navigationService).Verify(ns => ns.NavigateToDashboardAsync(), Times.Once());


        private static string notExistedInterviewId = "same id";
        private static PrefilledQuestionsViewModel viewModel;
        private static ILogger logger;
        private static IStatefulInterviewRepository interviewRepository;
        private static IViewModelNavigationService navigationService;
    }
}