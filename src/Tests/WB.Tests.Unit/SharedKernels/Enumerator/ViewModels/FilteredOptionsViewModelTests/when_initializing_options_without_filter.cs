using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredOptionsViewModelTests
{
    internal class when_initializing_options_without_filter : FilteredOptionsViewModelTestContext
    {
        Establish context = () =>
        {
            interviewId = "interview";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.IsSupportFilteringForOptions(questionId.Id) == false
            );

            options = new[]
            {
                new CategoricalOption() {Title = "a", Value = 1},
                new CategoricalOption() {Title = "b", Value = 2},
                new CategoricalOption() {Title = "c", Value = 3},
            }.ToList();

            var interview = Mock.Of<IStatefulInterview>(x => x.GetTopFilteredOptionsForQuestion(questionId, null, string.Empty, 200) == options);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            answerNotifier = new Mock<AnswerNotifier>();

            viewModel = CreateViewModel(questionnaireRepository: questionnaireStorage.Object, 
                interviewRepository: interviewRepository.Object,
                answerNotifier:  answerNotifier.Object);
        };

        Because of = () => viewModel.Init(interviewId, questionId, 200);

        It should_build_options = () =>
        {
            viewModel.GetOptions().ShouldNotBeNull();
            viewModel.GetOptions().Count().ShouldEqual(3);
        };

        It should_contains_all_options = () =>
        {
            viewModel.GetOptions().ShouldEqual(options);
        };

        It should_not_subscribe_model_in_answerNotify = () =>
        {
            answerNotifier.Verify(x => x.Init(interviewId), Times.Never());
        };

        static FilteredOptionsViewModel viewModel;
        static string interviewId;
        static Identity questionId;
        private static Guid questionGuid;
        private static List<CategoricalOption> options;
        private static Mock<AnswerNotifier> answerNotifier;
    }
}

