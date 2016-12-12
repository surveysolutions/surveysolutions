using Moq;
using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SideBarSectionViewModelTests
{
    internal class SideBarSectionViewModelTestsContext
    {
        protected static SideBarSectionViewModel CreateViewModel(IQuestionnaire questionnaire = null,
            IStatefulInterview interview = null)
        {
            Mock<IStatefulInterviewRepository> interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetReturnsDefault(interview);
            Mock<IQuestionnaireStorage> questionnaireRepository = new Mock<IQuestionnaireStorage>();
            questionnaireRepository.SetReturnsDefault(questionnaire);

            return new SideBarSectionViewModel(
                interviewRepository.Object, 
                Stub.SideBarSectionViewModelsFactory(),
                Mock.Of<IMvxMessenger>(),
                Create.ViewModel.DynamicTextViewModel(
                    interviewRepository: interviewRepository.Object));
        }
    }
}