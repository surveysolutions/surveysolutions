using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_toggling_answer_and_max_answers_count_reached : MultiOptionQuestionViewModelTestsContext
    {
        private Establish context = () =>
        {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == true
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == 1
                && _.ShouldQuestionSpecifyRosterSize(questionId.Id) == false
                && _.GetAnswerOptionsAsValues(questionId.Id) == new decimal[] { 1, 2 }
                && _.GetAnswerOptionTitle(questionId.Id, 1) == "item1"
                && _.GetAnswerOptionTitle(questionId.Id, 2) == "item2"
            );

            var multiOptionAnswer = Create.MultiOptionAnswer(questionGuid, Empty.RosterVector);
            multiOptionAnswer.SetAnswers(new[] {1m});

            var interview = Mock.Of<IStatefulInterview>(x => x.GetMultiOptionAnswer(questionId) == multiOptionAnswer);

            var questionnaireStorage = new Mock<IPlainQuestionnaireRepository>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object);

            viewModel.Init("blah", questionId, Create.NavigationState());
            viewModel.Options.Second().Checked = true;
        };

        Because of = async () => await viewModel.ToggleAnswerAsync(viewModel.Options.Second());

        It should_undo_checked_property = () => viewModel.Options.Second().Checked.ShouldBeFalse();

        private static MultiOptionQuestionViewModel viewModel;
        private static Identity questionId;
        private static Guid questionGuid;
    }
}