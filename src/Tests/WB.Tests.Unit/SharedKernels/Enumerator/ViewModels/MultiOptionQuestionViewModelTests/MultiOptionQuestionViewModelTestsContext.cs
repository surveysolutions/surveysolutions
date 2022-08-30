using System;
using Moq;
using MvvmCross.Base;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    [TestOf(typeof(CategoricalMultiViewModel))]
    internal class MultiOptionQuestionViewModelTestsContext : BaseMvvmCrossTest
    {
        protected static CategoricalMultiViewModel CreateViewModel(IUserInteractionService userInteractionService = null, 
            IQuestionnaireStorage questionnaireStorage = null, 
            IViewModelEventRegistry eventRegistry = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            IPrincipal principal = null, 
            AnsweringViewModel answeringViewModel = null, 
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewmodel = null,
            FilteredOptionsViewModel filteredOptionsViewModel = null,
            QuestionInstructionViewModel instructionViewModel = null,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher = null)
        {
            var questionnaireRepository = questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>();
            var statefulInterviewRepository = interviewRepository ?? Mock.Of<IStatefulInterviewRepository>();

            var liteEventRegistry = eventRegistry ?? Mock.Of<IViewModelEventRegistry>();

            return new CategoricalMultiViewModel(
                questionStateViewmodel ?? Create.ViewModel.QuestionState<MultipleOptionsQuestionAnswered>(liteEventRegistry, statefulInterviewRepository, questionnaireStorage),
                questionnaireRepository,
                liteEventRegistry,
                statefulInterviewRepository,
                principal ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                answeringViewModel ?? Mock.Of<AnsweringViewModel>(),
                filteredOptionsViewModel ?? Mock.Of<FilteredOptionsViewModel>(),
                instructionViewModel ?? Mock.Of<QuestionInstructionViewModel>(),
                Create.ViewModel.ThrottlingViewModel(),
                Create.Service.InterviewViewModelFactory());
        }
    }
}
