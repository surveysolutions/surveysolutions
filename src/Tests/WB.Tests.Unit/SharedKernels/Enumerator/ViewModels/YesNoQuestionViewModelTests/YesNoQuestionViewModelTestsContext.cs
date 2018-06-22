using System;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
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
    [NUnit.Framework.TestOf(typeof(YesNoQuestionViewModel))]
    public class YesNoQuestionViewModelTestsContext
    {
        protected static YesNoQuestionViewModel CreateViewModel(IUserInteractionService userInteractionService = null,
           IQuestionnaireStorage questionnaireStorage = null,
           ILiteEventRegistry eventRegistry = null,
           IStatefulInterviewRepository interviewRepository = null,
           IPrincipal principal = null,
           AnsweringViewModel answeringViewModel = null,
           QuestionStateViewModel<YesNoQuestionAnswered> questionStateViewmodel = null,
           FilteredOptionsViewModel filteredOptionsViewModel = null)
        {
            return new YesNoQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                Stub.MvxMainThreadDispatcher(),
                questionStateViewmodel ?? Mock.Of<QuestionStateViewModel<YesNoQuestionAnswered>>(x => x.Validity == Mock.Of<ValidityViewModel>()),
                answeringViewModel ?? Mock.Of<AnsweringViewModel>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                filteredOptionsViewModel ?? Mock.Of<FilteredOptionsViewModel>(),
                Create.ViewModel.QuestionInstructionViewModel()
            )
            {
                ThrottlePeriod = 0
            };
        }
    }
}
