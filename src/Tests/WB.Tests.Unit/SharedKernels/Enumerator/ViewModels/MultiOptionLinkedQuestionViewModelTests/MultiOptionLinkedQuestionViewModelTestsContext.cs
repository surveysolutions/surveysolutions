using System;
using MvvmCross.Platform.Core;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    [Subject(typeof(MultiOptionLinkedToQuestionQuestionViewModel))]
    internal class MultiOptionLinkedQuestionViewModelTestsContext
    {
        protected static MultiOptionLinkedToQuestionQuestionViewModel CreateViewModel(QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState = null, 
            AnsweringViewModel answering = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            IAnswerToStringService answerToStringService = null, 
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage = null, 
            IPrincipal userIdentity = null, 
            AnswerNotifier answerNotifier = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMainThreadDispatcher mainThreadDispatcher = null,
            IPlainQuestionnaireRepository questionnaireRepository = null)
        {
            return new MultiOptionLinkedToQuestionQuestionViewModel(questionState ?? Mock.Of<QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered>>(x => x.Validity == Mock.Of<ValidityViewModel>()),
                answering ?? Mock.Of<AnsweringViewModel>(),
                answerNotifier ?? Create.AnswerNotifier(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                answerToStringService ?? Create.AnswerToStringService(),
                questionnaireStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                userIdentity ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                mainThreadDispatcher ?? Stub.MvxMainThreadDispatcher(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>());
        }

        protected static MultiOptionLinkedToRosterQuestionViewModel CreateMultiOptionRosterLinkedQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null,
            IStatefulInterviewRepository interviewRepository = null,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage = null,
            IPrincipal userIdentity = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMainThreadDispatcher mainThreadDispatcher = null)
        {
            return
                new MultiOptionLinkedToRosterQuestionViewModel(
                    questionState ??
                    Mock.Of<QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered>>(
                        x => x.Validity == Mock.Of<ValidityViewModel>()),
                    answering ?? Mock.Of<AnsweringViewModel>(),
                    interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                    questionnaireStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                    userIdentity ??
                    Mock.Of<IPrincipal>(
                        x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                    eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                    mainThreadDispatcher ?? Stub.MvxMainThreadDispatcher());
        }
        protected static MultiOptionLinkedToRosterQuestionViewModel CreateMultiOptionRosterLinkedQuestionViewModel(QuestionnaireModel questionnaire,
          IStatefulInterview statefulInterview)
        {
            return CreateMultiOptionRosterLinkedQuestionViewModel(questionnaireStorage: Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(Moq.It.IsAny<string>()) == questionnaire),
                interviewRepository: Mock.Of<IStatefulInterviewRepository>(x => x.Get(Moq.It.IsAny<string>()) == statefulInterview));
        }
        protected static MultiOptionLinkedToQuestionQuestionViewModel CreateViewModel(QuestionnaireModel questionnaire, 
            IStatefulInterview statefulInterview,
            AnswerNotifier answerNotifier = null)
        {
            return CreateViewModel(questionnaireStorage: Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(Moq.It.IsAny<string>()) == questionnaire),
                interviewRepository: Mock.Of<IStatefulInterviewRepository>(x => x.Get(Moq.It.IsAny<string>()) == statefulInterview),
                answerNotifier: answerNotifier);
        }
    }
}