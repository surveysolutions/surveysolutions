using System;
using Moq;
using MvvmCross.Base;
using MvvmCross.Tests;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    [NUnit.Framework.TestOf(typeof(CategoricalMultiLinkedToQuestionViewModel))]
    internal class MultiOptionLinkedQuestionViewModelTestsContext : MvxIoCSupportingTest
    {
        public MultiOptionLinkedQuestionViewModelTestsContext()
        {
            base.Setup();
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(Stub.MvxMainThreadAsyncDispatcher());
        }

        protected static CategoricalMultiLinkedToQuestionViewModel CreateViewModel(QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState = null, 
            AnsweringViewModel answering = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            IQuestionnaireStorage questionnaireStorage = null, 
            IPrincipal userIdentity = null, 
            AnswerNotifier answerNotifier = null,
            IViewModelEventRegistry eventRegistry = null,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher = null)
        {
            var questionnaireRepository = questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>();
            var statefulInterviewRepository = interviewRepository ?? Mock.Of<IStatefulInterviewRepository>();
            var liteEventRegistry = eventRegistry ?? Mock.Of<IViewModelEventRegistry>();

            return new CategoricalMultiLinkedToQuestionViewModel(
                questionState ?? Create.ViewModel.QuestionState<MultipleOptionsLinkedQuestionAnswered>(liteEventRegistry, statefulInterviewRepository, questionnaireStorage),
                questionnaireRepository,
                liteEventRegistry,
                statefulInterviewRepository,
                userIdentity ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                answering ?? Mock.Of<AnsweringViewModel>(),
                Mock.Of<QuestionInstructionViewModel>(),
                Create.ViewModel.ThrottlingViewModel(),
                Create.Service.InterviewViewModelFactory());
        }

        protected static CategoricalMultiLinkedToRosterTitleViewModel CreateMultiOptionRosterLinkedQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null,
            IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IPrincipal userIdentity = null,
            IViewModelEventRegistry eventRegistry = null,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher = null)
        {
            var liteEventRegistry = eventRegistry ?? Mock.Of<IViewModelEventRegistry>();
            var questionnaireRepository = questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>();

            var statefulInterviewRepository = interviewRepository ?? Mock.Of<IStatefulInterviewRepository>();
            return
                new CategoricalMultiLinkedToRosterTitleViewModel(
                    questionState ?? Create.ViewModel.QuestionState<MultipleOptionsLinkedQuestionAnswered>(liteEventRegistry, statefulInterviewRepository, questionnaireStorage),
                    questionnaireRepository,
                    liteEventRegistry,
                    statefulInterviewRepository,
                    userIdentity ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                    answering ?? Mock.Of<AnsweringViewModel>(),
                    Mock.Of<QuestionInstructionViewModel>(),
                    Create.ViewModel.ThrottlingViewModel(),
                    Create.Service.InterviewViewModelFactory());
        }
    }
}
