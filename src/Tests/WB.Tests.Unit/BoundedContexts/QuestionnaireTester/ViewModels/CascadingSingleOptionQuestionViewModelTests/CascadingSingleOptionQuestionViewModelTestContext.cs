using System;
using System.Diagnostics;

using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.IoC;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross;
using Cirrious.MvvmCross.Platform;

using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class CascadingSingleOptionQuestionViewModelTestContext 
    {
        protected static CascadingSingleOptionQuestionViewModel CreateCascadingSingleOptionQuestionViewModel(
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel = null,
            AnsweringViewModel answering = null,
            IPrincipal principal = null,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            ILiteEventRegistry eventRegistry = null)
        {
            return new CascadingSingleOptionQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(), 
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(), 
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<SingleOptionQuestionAnswered>>(), 
                answering ?? Mock.Of<AnsweringViewModel>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>());
        }
    }
}
