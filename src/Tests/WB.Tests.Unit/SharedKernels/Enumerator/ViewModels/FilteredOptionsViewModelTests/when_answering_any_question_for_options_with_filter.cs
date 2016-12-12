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
    internal class when_answering_any_question_for_options_with_filter : FilteredOptionsViewModelTestContext
    {
        Establish context = () =>
        {
            interviewId = "interview";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.IsSupportFilteringForOptions(questionId.Id) == true
            );

            options = new[]
            {
                new CategoricalOption() {Title = "a", Value = 1},
                new CategoricalOption() {Title = "b", Value = 2},
                new CategoricalOption() {Title = "c", Value = 3},
            }.ToList();
            filteredOptions = new[]
            {
                new CategoricalOption() {Title = "b", Value = 2}, 
                new CategoricalOption() {Title = "c", Value = 3},
            }.ToList();
            interview = Mock.Of<IStatefulInterview>(x =>
                x.GetTopFilteredOptionsForQuestion(questionId, null, string.Empty, 200) == options
                && x.GetTopFilteredOptionsForQuestion(questionId, null, "a", 200) == filteredOptions);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            answerNotifier = new Mock<AnswerNotifier>();

            viewModel = CreateViewModel(questionnaireRepository: questionnaireStorage.Object, 
                interviewRepository: interviewRepository.Object,
                answerNotifier:  answerNotifier.Object);
            viewModel.Init(interviewId, questionId, 200);
        };

        Because of = () => resultOptions = viewModel.GetOptions("a");

        It should_build_options = () =>
        {
            resultOptions.ShouldNotBeNull();
            resultOptions.Count().ShouldEqual(2);
        };

        private It should_contains_all_filtered_options = () =>
        {
            resultOptions.ShouldEqual(filteredOptions);
        };

        It should_subscribe_model_in_answerNotify = () =>
        {
            answerNotifier.Verify(x => x.Init(interviewId), Times.Once);
        };

        It should_get_twice_list_of_options = () =>
        {
            Mock.Get(interview).Verify(x => x.GetTopFilteredOptionsForQuestion(questionId, null, String.Empty,200), Times.Once);
            Mock.Get(interview).Verify(x => x.GetTopFilteredOptionsForQuestion(questionId, null, "a", 200), Times.Once);
        };

        static FilteredOptionsViewModel viewModel;
        static string interviewId;
        static Identity questionId;
        private static Guid questionGuid;
        private static List<CategoricalOption> options;
        private static List<CategoricalOption> resultOptions;
        private static List<CategoricalOption> filteredOptions;
        private static Mock<AnswerNotifier> answerNotifier;
        private static IStatefulInterview interview;
    }
}

