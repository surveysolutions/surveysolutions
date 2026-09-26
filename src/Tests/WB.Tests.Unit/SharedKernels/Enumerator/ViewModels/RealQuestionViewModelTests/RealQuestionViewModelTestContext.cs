using System.Threading.Tasks;
using Moq;
using MvvmCross.Base;
using MvvmCross.Tests;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RealQuestionViewModelTests
{
    public class RealQuestionViewModelTestContext : MvxIoCSupportingTest
    {
        protected static readonly string InterviewId = "Some interview id";
        protected static readonly string QuestionnaireId = "Questionnaire id";
        protected static readonly Identity QuestionIdentity = Create.Entity.Identity(System.Guid.NewGuid());
        protected static readonly NavigationState NavigationState = Create.Other.NavigationState();
        protected static Mock<ValidityViewModel> ValidityModelMock;
        protected static Mock<AnsweringViewModel> AnsweringViewModelMock;
        protected static Mock<QuestionStateViewModel<NumericRealQuestionAnswered>> QuestionStateMock;

        public RealQuestionViewModelTestContext()
        {
            base.Setup();
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(Stub.MvxMainThreadAsyncDispatcher());
        }

        protected static void SetUp()
        {
            ValidityModelMock = new Mock<ValidityViewModel>();
            QuestionStateMock = new Mock<QuestionStateViewModel<NumericRealQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            QuestionStateMock.Setup(x => x.Validity).Returns(ValidityModelMock.Object);
            AnsweringViewModelMock = new Mock<AnsweringViewModel> { DefaultValue = DefaultValue.Mock };
        }

        protected static RealQuestionViewModel CreateRealQuestionViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            SpecialValuesViewModel specialValues = null)
        {
            var userId = System.Guid.NewGuid();
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity && _.IsAuthenticated == true);
            var specialMock = new Mock<SpecialValuesViewModel> { DefaultValue = DefaultValue.Mock };
            specialMock.SetReturnsDefault(Task.CompletedTask);

            return new RealQuestionViewModel(
                principal,
                interviewRepository,
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                Mock.Of<QuestionInstructionViewModel>(),
                questionnaireRepository,
                Mock.Of<IViewModelEventRegistry>(),
                specialValues ?? specialMock.Object,
                Create.ViewModel.ThrottlingViewModel());
        }
    }
}
