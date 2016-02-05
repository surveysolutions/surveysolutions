using System;
using MvvmCross.Test.Core;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.IntegerQuestionViewModelTests
{
    internal class IntegerQuestionViewModelTestContext : MvxIoCSupportingTest
    {
        public IntegerQuestionViewModelTestContext()
        {
            base.Setup();
        }
        
        public static IntegerQuestionViewModel CreateIntegerQuestionViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            IUserInteractionService userInteractionService = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            QuestionStateMock.Setup(x => x.Validity).Returns(ValidityModelMock.Object);

            return new IntegerQuestionViewModel(
                principal,
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                QuestionStateMock.Object,
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                AnsweringViewModelMock.Object,
                Mock.Of<ILiteEventRegistry>());
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