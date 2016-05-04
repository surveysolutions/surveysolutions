﻿using System;
using Moq;
using MvvmCross.Test.Core;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class TextListQuestionViewModelTestContext : MvxIoCSupportingTest
    {
        public TextListQuestionViewModelTestContext()
        {
            base.Setup();
        }

        protected static readonly Identity questionIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);

        protected static IPlainQuestionnaireRepository SetupQuestionnaireRepositoryWithListQuestion(bool isRosterSizeQuestion = false, int? maxAnswerCount = 5)
        {
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionSpecifyRosterSize(questionIdentity.Id) == isRosterSizeQuestion
                && _.GetMaxSelectedAnswerOptions(questionIdentity.Id) == maxAnswerCount
            );
            return Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire);
        }

        protected static TextListQuestionViewModel CreateTextListQuestionViewModel(
            QuestionStateViewModel<TextListQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            IPrincipal principal = null,
            IPlainQuestionnaireRepository questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            
            IUserInteractionService userInteractionService = null)
        {
            return new TextListQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<TextListQuestionAnswered>>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                answering ?? Mock.Of<AnsweringViewModel>());
        }
    }
}
