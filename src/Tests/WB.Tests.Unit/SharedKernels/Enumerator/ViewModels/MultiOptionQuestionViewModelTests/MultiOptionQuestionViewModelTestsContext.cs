using System;
using Machine.Specifications;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    [Subject(typeof(MultiOptionQuestionViewModel))]
    [TestOf(typeof(MultiOptionQuestionViewModel))]
    internal class MultiOptionQuestionViewModelTestsContext
    {
        protected static MultiOptionQuestionViewModel CreateViewModel(IUserInteractionService userInteractionService = null, 
            IQuestionnaireStorage questionnaireStorage = null, 
            ILiteEventRegistry eventRegistry = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            IPrincipal principal = null, 
            AnsweringViewModel answeringViewModel = null, 
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewmodel = null,
            FilteredOptionsViewModel filteredOptionsViewModel = null,
            QuestionInstructionViewModel instructionViewModel = null)
        {
            return new MultiOptionQuestionViewModel(
                questionStateViewmodel ?? Mock.Of<QuestionStateViewModel<MultipleOptionsQuestionAnswered>>(x => x.Validity == Mock.Of<ValidityViewModel>()),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                principal ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                answeringViewModel ?? Mock.Of<AnsweringViewModel>(),
                filteredOptionsViewModel ?? Mock.Of<FilteredOptionsViewModel>(),
                instructionViewModel ?? Mock.Of<QuestionInstructionViewModel>());
        }
    }
}