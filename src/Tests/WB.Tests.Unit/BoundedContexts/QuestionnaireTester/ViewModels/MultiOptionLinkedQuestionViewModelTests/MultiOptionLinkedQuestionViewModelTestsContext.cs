using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    [Subject(typeof(MultiOptionLinkedQuestionViewModel))]
    public class MultiOptionLinkedQuestionViewModelTestsContext
    {
        protected static MultiOptionLinkedQuestionViewModel CreateViewModel(QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState = null, 
            AnsweringViewModel answering = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            IAnswerToStringService answerToStringService = null, 
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage = null, 
            IPrincipal userIdentity = null, 
            ILiteEventRegistry eventRegistry = null)
        {
            return new MultiOptionLinkedQuestionViewModel(questionState ?? Mock.Of<QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered>>(),
                answering ?? Mock.Of<AnsweringViewModel>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                answerToStringService ?? Create.AnswerToStringService(),
                questionnaireStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                userIdentity ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>());
        }
    }
}