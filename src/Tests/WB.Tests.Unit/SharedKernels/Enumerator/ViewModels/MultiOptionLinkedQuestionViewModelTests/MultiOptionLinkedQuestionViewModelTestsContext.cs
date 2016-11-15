using System;
using MvvmCross.Platform.Core;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
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
            IQuestionnaireStorage questionnaireStorage = null, 
            IPrincipal userIdentity = null, 
            AnswerNotifier answerNotifier = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMainThreadDispatcher mainThreadDispatcher = null)
        {
            return new MultiOptionLinkedToQuestionQuestionViewModel(
                questionState ?? Mock.Of<QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered>>(x => x.Validity == Mock.Of<ValidityViewModel>()),
                answering ?? Mock.Of<AnsweringViewModel>(),
                Mock.Of<QuestionInstructionViewModel>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                userIdentity ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                mainThreadDispatcher ?? Stub.MvxMainThreadDispatcher());
        }

        protected static MultiOptionLinkedToRosterQuestionViewModel CreateMultiOptionRosterLinkedQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null,
            IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
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
                    Mock.Of<QuestionInstructionViewModel>(),
                    interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                    questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                    userIdentity ??
                    Mock.Of<IPrincipal>(
                        x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                    eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                    mainThreadDispatcher ?? Stub.MvxMainThreadDispatcher());
        }
        protected static MultiOptionLinkedToRosterQuestionViewModel CreateMultiOptionRosterLinkedQuestionViewModel(
            IQuestionnaire questionnaire,
            IStatefulInterview statefulInterview)
        {
            var questionnaireRepository = new Mock<IQuestionnaireStorage>();
            questionnaireRepository.SetReturnsDefault(questionnaire);

            return CreateMultiOptionRosterLinkedQuestionViewModel(questionnaireStorage: questionnaireRepository.Object,
                interviewRepository: Mock.Of<IStatefulInterviewRepository>(x => x.Get(Moq.It.IsAny<string>()) == statefulInterview));
        }

        protected static MultiOptionLinkedToQuestionQuestionViewModel CreateViewModel(
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