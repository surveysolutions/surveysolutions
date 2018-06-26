using System;
using Moq;
using MvvmCross.Base;
using MvvmCross.Tests;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class TextListQuestionViewModelTestContext : MvxIoCSupportingTest
    {
        public TextListQuestionViewModelTestContext()
        {
            base.Setup();
        }

        protected static readonly Identity questionIdentity =
            Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);

        protected static IQuestionnaireStorage SetupQuestionnaireRepositoryWithListQuestion(
            bool isRosterSizeQuestion = false, int? maxAnswerCount = 5)
        {
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.IsRosterSizeQuestion(questionIdentity.Id) == isRosterSizeQuestion
                   && _.GetMaxSelectedAnswerOptions(questionIdentity.Id) == maxAnswerCount
            );
            return Mock.Of<IQuestionnaireStorage>(x =>
                x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
        }

        protected static TextListQuestionViewModel CreateTextListQuestionViewModel(
            QuestionStateViewModel<TextListQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            IPrincipal principal = null,
            IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            IUserInteractionService userInteractionService = null,
            IMvxMainThreadDispatcher mainThreadDispatcher = null)
        {
            return new TextListQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(x => x.IsAuthenticated == true),
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<TextListQuestionAnswered>>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                answering ?? Mock.Of<AnsweringViewModel>(),
                instructionViewModel: Mock.Of<QuestionInstructionViewModel>(),
                mainThreadDispatcher: mainThreadDispatcher ?? Stub.MvxMainThreadDispatcher());
        }
    }
}
