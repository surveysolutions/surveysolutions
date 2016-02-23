using Moq;
using MvvmCross.Test.Core;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.PrefilledQuestionsViewModelTests
{
    internal class PrefilledQuestionsViewModelTestContext : MvxIoCSupportingTest
    {
        public PrefilledQuestionsViewModelTestContext()
        {
            base.Setup();
        }

        public static PrefilledQuestionsViewModel CreatePrefilledQuestionsViewModel(
            IInterviewViewModelFactory interviewViewModelFactory = null,
            IStatefulInterviewRepository interviewRepository = null,
            IViewModelNavigationService viewModelNavigationService = null,
            ILogger logger = null,
            IPrincipal principal = null)
        {
            return new PrefilledQuestionsViewModel(
                interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                Mock.Of<IPlainQuestionnaireRepository>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                logger ?? Mock.Of<ILogger>(),
                principal ?? Mock.Of<IPrincipal>());
        }
    }
}