using Moq;
using MvvmCross.Plugins.Messenger;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SideBarSectionViewModelTests
{
    internal class SideBarSectionViewModelTestsContext
    {
        protected static SideBarSectionViewModel CreateViewModel(QuestionnaireModel questionnaire = null,
            IStatefulInterview interview = null)
        {
            Mock<IStatefulInterviewRepository> interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetReturnsDefault(interview);
            Mock<IPlainQuestionnaireRepository> questionnaireRepository = new Mock<IPlainQuestionnaireRepository>();
            questionnaireRepository.SetReturnsDefault(questionnaire);

            return new SideBarSectionViewModel(interviewRepository.Object, 
                questionnaireRepository.Object, 
                Create.SubstitutionService(), 
                Create.LiteEventRegistry(), 
                Stub.SideBarSectionViewModelsFactory(),
                Mock.Of<IMvxMessenger>());
        }
    }
}