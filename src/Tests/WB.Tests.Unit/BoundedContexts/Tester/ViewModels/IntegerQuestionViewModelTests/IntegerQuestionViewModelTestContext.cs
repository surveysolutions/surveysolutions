using System;

using Chance.MvvmCross.Plugins.UserInteraction;

using Moq;

using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.IntegerQuestionViewModelTests
{
    public class IntegerQuestionViewModelTestContext
    {
        public static IntegerQuestionViewModel CreateIntegerQuestionViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            IUserInteraction userInteraction = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            QuestionStateMock.Setup(x => x.Validity).Returns(ValidityModelMock.Object);

            return new IntegerQuestionViewModel(
                principal,
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                QuestionStateMock.Object,
                userInteraction ?? Mock.Of<IUserInteraction>(),
                AnsweringViewModelMock.Object);
        }

        protected static void SetUp()
        {
            ValidityModelMock = new Mock<ValidityViewModel>();
            navigationState = Create.NavigationState();
            QuestionStateMock = new Mock<QuestionStateViewModel<NumericIntegerQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            AnsweringViewModelMock = new Mock<AnsweringViewModel> { DefaultValue = DefaultValue.Mock };
            EventRegistry = new Mock<ILiteEventRegistry>();
        }

        protected static Identity questionIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[] { 1, 2 });

        protected static NavigationState navigationState;

        protected static Mock<ValidityViewModel> ValidityModelMock;

        protected static Mock<QuestionStateViewModel<NumericIntegerQuestionAnswered>> QuestionStateMock;

        protected static Mock<AnsweringViewModel> AnsweringViewModelMock;

        protected static Mock<ILiteEventRegistry> EventRegistry;

        protected static readonly string interviewId = "Some interviewId";

        protected static readonly string questionnaireId = "Questionnaire Id";

        protected static Guid interviewGuid = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        protected static readonly Guid userId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}