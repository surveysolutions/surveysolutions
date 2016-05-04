using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
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
            IPlainQuestionnaireRepository questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null)
        {
            return new FilteredSingleOptionQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<ILiteEventRegistry>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<SingleOptionQuestionAnswered>>(),
                answering ?? Mock.Of<AnsweringViewModel>());
        }

        protected static IPlainQuestionnaireRepository SetupQuestionnaireRepositoryWithFilteredQuestion()
        {
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetAnswerOptionsAsValues(questionIdentity.Id) == new decimal[] { 1, 2, 3, 4, 5 }
                && _.GetAnswerOptionTitle(questionIdentity.Id, 1) == "abc"
                && _.GetAnswerOptionTitle(questionIdentity.Id, 2) == "bbc"
                && _.GetAnswerOptionTitle(questionIdentity.Id, 3) == "bbc"
                && _.GetAnswerOptionTitle(questionIdentity.Id, 4) == "bbaé"
                && _.GetAnswerOptionTitle(questionIdentity.Id, 5) == "cccé"
            );
            return Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire);
        }

        protected static readonly Identity questionIdentity = Create.Identity(Id.g6, Create.RosterVector());
    }
}