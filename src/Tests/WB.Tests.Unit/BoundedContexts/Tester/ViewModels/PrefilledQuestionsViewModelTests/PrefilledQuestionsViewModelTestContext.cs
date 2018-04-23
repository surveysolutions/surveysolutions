using Moq;
using MvvmCross.Tests;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Tests.Abc;

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
            IPrincipal principal = null,
            ICompositeCollectionInflationService compositeCollectionInflationService = null,
            VibrationViewModel vibrationViewModel = null)
        {
            return new PrefilledQuestionsViewModel(
                interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                logger ?? Mock.Of<ILogger>(),
                principal ?? Mock.Of<IPrincipal>(),
                Mock.Of<ICommandService>(),
                Mock.Of<ICompositeCollectionInflationService>(),
                vibrationViewModel ?? Create.ViewModel.VibrationViewModel(),
                null);
        }
    }
}
