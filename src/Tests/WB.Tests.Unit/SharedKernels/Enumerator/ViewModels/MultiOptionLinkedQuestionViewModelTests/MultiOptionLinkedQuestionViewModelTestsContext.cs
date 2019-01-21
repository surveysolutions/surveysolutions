using System;
using Moq;
using MvvmCross.Base;
using MvvmCross.Tests;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
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

        protected static CategoricalMultiLinkedToRosterTitleViewModel CreateViewModel(QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState = null, 
            AnsweringViewModel answering = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            IQuestionnaireStorage questionnaireStorage = null, 
            IPrincipal userIdentity = null, 
            AnswerNotifier answerNotifier = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher = null)
        {
            var questionnaireRepository = questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>();
            var statefulInterviewRepository = interviewRepository ?? Mock.Of<IStatefulInterviewRepository>();
            var liteEventRegistry = eventRegistry ?? Mock.Of<ILiteEventRegistry>();

            return new CategoricalMultiLinkedToRosterTitleViewModel(
                questionState ?? Create.ViewModel.QuestionState<MultipleOptionsLinkedQuestionAnswered>(liteEventRegistry, statefulInterviewRepository, questionnaireStorage),
                questionnaireRepository,
                liteEventRegistry,
                statefulInterviewRepository,
                userIdentity ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                answering ?? Mock.Of<AnsweringViewModel>(),
                Mock.Of<QuestionInstructionViewModel>(),
                Create.ViewModel.ThrottlingViewModel());
        }

        protected static CategoricalMultiLinkedToQuestionViewModel CreateMultiOptionRosterLinkedQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null,
            IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IPrincipal userIdentity = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher = null)
        {
            return
                new CategoricalMultiLinkedToQuestionViewModel(
                    questionState ?? Mock.Of<QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered>>(x => x.Validity == Mock.Of<ValidityViewModel>()),
                    questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                    eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                    interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                    userIdentity ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                    answering ?? Mock.Of<AnsweringViewModel>(),
                    Mock.Of<QuestionInstructionViewModel>(),
                    Create.ViewModel.ThrottlingViewModel());
        }

        protected static CategoricalMultiLinkedToQuestionViewModel CreateViewModel(
            IQuestionnaire questionnaire, 
            IStatefulInterview statefulInterview,
            AnswerNotifier answerNotifier = null)
        {
            return CreateViewModel(questionnaireStorage: Mock.Of<IQuestionnaireStorage>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>()) == questionnaire),
                interviewRepository: Mock.Of<IStatefulInterviewRepository>(x => x.Get(Moq.It.IsAny<string>()) == statefulInterview),
                answerNotifier: answerNotifier);
        }
    }
}
