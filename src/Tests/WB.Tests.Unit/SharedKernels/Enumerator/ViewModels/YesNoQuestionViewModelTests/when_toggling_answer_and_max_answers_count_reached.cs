using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_toggling_answer_and_max_answers_count_reached : YesNoQuestionViewModelTestsContext
    {
        private Establish context = () =>
        {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == false
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == 2
                && _.ShouldQuestionSpecifyRosterSize(questionId.Id) == false
                && _.GetAnswerOptionsAsValues(questionId.Id) == new decimal[] { 1, 2, 3, 4, 5 }
                && _.GetAnswerOptionTitle(questionId.Id, 1) == "item1"
                && _.GetAnswerOptionTitle(questionId.Id, 2) == "item2"
                && _.GetAnswerOptionTitle(questionId.Id, 3) == "item3"
                && _.GetAnswerOptionTitle(questionId.Id, 4) == "item4"
                && _.GetAnswerOptionTitle(questionId.Id, 5) == "item5"
            );

            var yesNoAnswer = Create.YesNoAnswer(questionGuid, Empty.RosterVector);
            yesNoAnswer.SetAnswers(new[]
            {
                new AnsweredYesNoOption(5, true),
                new AnsweredYesNoOption(2, false),
                new AnsweredYesNoOption(4, true),
            });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoAnswer(questionId) == yesNoAnswer);

            var questionnaireStorage = new Mock<IPlainQuestionnaireRepository>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object);

            viewModel.Init("blah", questionId, Create.NavigationState());
            viewModel.Options.First().Selected = true;
        };

        private Because of = async () => await viewModel.ToggleAnswerAsync(viewModel.Options.First());

        private It should_undo_checked_property = () => viewModel.Options.First().YesSelected.ShouldBeFalse();

        private static YesNoQuestionViewModel viewModel;
        private static Identity questionId;
        private static Guid questionGuid;
    }
}