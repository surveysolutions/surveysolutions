using System;
using System.Collections.Generic;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class FilteredSingleOptionQuestionViewModelTestsContext
    {
        protected static FilteredSingleOptionQuestionViewModel CreateFilteredSingleOptionQuestionViewModel(
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            IPrincipal principal = null,
            IStatefulInterviewRepository interviewRepository = null,
            IOptionsRepository optionsRepository = null)
        {
            return new FilteredSingleOptionQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<ILiteEventRegistry>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<SingleOptionQuestionAnswered>>(),
                answering ?? Mock.Of<AnsweringViewModel>(),
                optionsRepository ?? Mock.Of<IOptionsRepository>());
        }

        protected static IOptionsRepository SetupOptionsRepositoryForQuestionnaire(Guid questionId)
        {
            var options = new List<CategoricalQuestionOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "abc"),
                Create.Entity.CategoricalQuestionOption(2, "bbc"),
                Create.Entity.CategoricalQuestionOption(3, "bbc"),
                Create.Entity.CategoricalQuestionOption(4, "bbaé"),
                Create.Entity.CategoricalQuestionOption(5, "cccé"),
            };

            Mock<IOptionsRepository> optionsRepository = new Mock<IOptionsRepository>();

            optionsRepository
                .Setup(x => x.GetQuestionOptions(questionnaireId, questionId))
                .Returns(options);

            return optionsRepository.Object;
        }

        protected static readonly Identity questionIdentity = Create.Entity.Identity(Id.g6, Create.Entity.RosterVector());

        protected static readonly QuestionnaireIdentity questionnaireId = Create.Entity.QuestionnaireIdentity(Id.g9, 1);
    }
}