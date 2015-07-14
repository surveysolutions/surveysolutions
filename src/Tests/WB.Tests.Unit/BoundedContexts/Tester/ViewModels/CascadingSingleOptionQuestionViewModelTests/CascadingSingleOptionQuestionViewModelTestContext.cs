using System;
using System.Collections.Generic;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class CascadingSingleOptionQuestionViewModelTestContext 
    {
        protected static CascadingSingleOptionQuestionViewModel CreateCascadingSingleOptionQuestionViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            return new CascadingSingleOptionQuestionViewModel(
                principal, 
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(), 
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                EventRegistry.Object);
        }

        protected static void SetUp()
        {
            navigationState = Create.NavigationState();
            QuestionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            AnsweringViewModelMock = new Mock<AnsweringViewModel> { DefaultValue = DefaultValue.Mock };
            EventRegistry = new Mock<ILiteEventRegistry>();
        }

        protected static Identity questionIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[] { 1, 2 });

        protected static Identity parentIdentity = Create.Identity(Guid.Parse("22222222222222222222222222222222"), new decimal[] { 1 });

        protected static NavigationState navigationState;

        protected static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> QuestionStateMock;

        protected static Mock<AnsweringViewModel> AnsweringViewModelMock;

        protected static Mock<ILiteEventRegistry> EventRegistry;

        protected static List<CascadingOptionModel> Options = new List<CascadingOptionModel>
        {
            Create.CascadingOptionModel(1, "title abc 1", 1),
            Create.CascadingOptionModel(2, "title def 2", 1),
            Create.CascadingOptionModel(3, "title klo 3", 1),
            Create.CascadingOptionModel(4, "title gha 4", 2),
            Create.CascadingOptionModel(5, "title ccc 5", 2),
            Create.CascadingOptionModel(6, "title ebw 6", 2)
        };

        protected static readonly string interviewId = "Some interviewId";

        protected static readonly string questionnaireId = "Questionnaire Id";

        protected static Guid interviewGuid = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        protected static readonly Guid userId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}
