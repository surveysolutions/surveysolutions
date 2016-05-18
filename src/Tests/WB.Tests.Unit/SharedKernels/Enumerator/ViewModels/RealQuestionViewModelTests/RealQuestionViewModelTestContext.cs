using System;
using Moq;
using MvvmCross.Test.Core;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RealQuestionViewModelTests
{
    internal class RealQuestionViewModelTestContext : MvxIoCSupportingTest
    {
        public RealQuestionViewModelTestContext()
        {
            base.Setup();
        }
        
        public static RealQuestionViewModel CreateViewModel(
            IPlainQuestionnaireRepository questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            ValidityViewModel validityViewModel = null,
            AnsweringViewModel answeringViewModel = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == Guid.Parse("ffffffffffffffffffffffffffffffff"));
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            var questionStateMock = new Mock<QuestionStateViewModel<NumericRealQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            questionStateMock.Setup(x => x.Validity).Returns(validityViewModel ?? new Mock<ValidityViewModel>().Object);
            
            return new RealQuestionViewModel(
                principal,
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionStateMock.Object,
                answeringViewModel ?? new Mock<AnsweringViewModel>().Object,
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<ILiteEventRegistry>());
        }
        
        protected static IPlainQuestionnaireRepository SetupQuestionnaireRepositoryWithNumericQuestion(Guid questionId, bool isRosterSize = true)
        {
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionSpecifyRosterSize(questionId) == isRosterSize
            );
            return Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire);
        }
    }
}