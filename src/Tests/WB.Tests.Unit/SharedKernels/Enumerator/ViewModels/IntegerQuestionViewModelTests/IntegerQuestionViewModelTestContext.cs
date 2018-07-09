using System;
using MvvmCross.Test.Core;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.IntegerQuestionViewModelTests
{
    internal class IntegerQuestionViewModelTestContext : MvxIoCSupportingTest
    {
        public IntegerQuestionViewModelTestContext()
        {
            base.Setup();
        }
        
        public static IntegerQuestionViewModel CreateIntegerQuestionViewModel(
            IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            IUserInteractionService userInteractionService = null,
            SpecialValuesViewModel specialValuesViewModel = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            QuestionStateMock.Setup(x => x.Validity).Returns(ValidityModelMock.Object);

            return new IntegerQuestionViewModel(
                principal,
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                QuestionStateMock.Object,
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                AnsweringViewModelMock.Object,
                Mock.Of<QuestionInstructionViewModel>(),
                Mock.Of<ILiteEventRegistry>(),
                specialValuesViewModel ?? Mock.Of<SpecialValuesViewModel>())
            {
                ThrottlePeriod = 0
            };
        }

        protected static void SetUp()
        {
            ValidityModelMock = new Mock<ValidityViewModel>();
            navigationState = Create.Other.NavigationState();
            QuestionStateMock = new Mock<QuestionStateViewModel<NumericIntegerQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            AnsweringViewModelMock = new Mock<AnsweringViewModel> { DefaultValue = DefaultValue.Mock };
            EventRegistry = new Mock<ILiteEventRegistry>();
        }

        protected static IQuestionnaireStorage SetupQuestionnaireRepositoryWithNumericQuestion(bool isRosterSize = true, bool isLongRosterSize = false)
        {
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.IsRosterSizeQuestion(questionIdentity.Id) == isRosterSize
                && _.IsQuestionIsRosterSizeForLongRoster(questionIdentity.Id) == isLongRosterSize
            );
            return Mock.Of<IQuestionnaireStorage>(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
        }

        protected static Identity questionIdentity = Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[] { 1, 2 });

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
