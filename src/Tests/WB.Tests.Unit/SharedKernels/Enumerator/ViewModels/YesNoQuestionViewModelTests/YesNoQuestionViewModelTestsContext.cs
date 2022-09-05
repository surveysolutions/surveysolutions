using System;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    [NUnit.Framework.TestOf(typeof(CategoricalYesNoViewModel))]
    public class YesNoQuestionViewModelTestsContext : BaseMvvmCrossTest
    {
        protected static CategoricalYesNoViewModel CreateViewModel(
           IQuestionnaireStorage questionnaireStorage = null,
           IViewModelEventRegistry eventRegistry = null,
           IStatefulInterviewRepository interviewRepository = null,
           IPrincipal principal = null,
           AnsweringViewModel answeringViewModel = null,
           QuestionStateViewModel<YesNoQuestionAnswered> questionStateViewmodel = null,
           FilteredOptionsViewModel filteredOptionsViewModel = null,
           ThrottlingViewModel throttlingModel = null,
           IInterviewViewModelFactory viewModelFactory = null,
           IUserInteractionService userInteraction = null)
        {
            userInteraction = userInteraction ?? Mock.Of<IUserInteractionService>();

            var mockOfViewModelFactory = new Mock<IInterviewViewModelFactory>();
            mockOfViewModelFactory.Setup(x => x.GetNew<CategoricalYesNoOptionViewModel>()).Returns(() =>
                new CategoricalYesNoOptionViewModel(userInteraction, Create.ViewModel.AttachmentViewModel()));

            var liteEventRegistry = eventRegistry ?? Mock.Of<IViewModelEventRegistry>();

            var statefulInterviewRepository = interviewRepository ?? Mock.Of<IStatefulInterviewRepository>();
            var questionnaireRepository = questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>();

            return new CategoricalYesNoViewModel(
                questionStateViewmodel ?? Create.ViewModel.QuestionState<YesNoQuestionAnswered>(liteEventRegistry, statefulInterviewRepository, questionnaireStorage),
                questionnaireRepository,
                liteEventRegistry,
                statefulInterviewRepository,
                principal ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                userInteraction: userInteraction,
                answeringViewModel ?? Mock.Of<AnsweringViewModel>(),
                Create.ViewModel.QuestionInstructionViewModel(),
                throttlingModel ?? Create.ViewModel.ThrottlingViewModel(),
                filteredOptionsViewModel ?? Mock.Of<FilteredOptionsViewModel>(),
                Create.Service.InterviewViewModelFactory());
        }
    }
}
