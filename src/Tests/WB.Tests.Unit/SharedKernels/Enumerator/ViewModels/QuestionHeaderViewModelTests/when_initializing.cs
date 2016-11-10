using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_initializing : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            substitutedQuesiton = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            substitutionTargetId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
           
            var questionnaireMock = Mock.Of<IQuestionnaire>(_
                => _.GetQuestionTitle(substitutionTargetId) == "title with %subst%"
                && _.GetQuestionInstruction(substitutionTargetId) == "Instruction"
                && _.GetQuestionIdByVariable("blah") == substitutionTargetId
                && _.GetQuestionIdByVariable("subst") == substitutedQuesiton
                && _.HasQuestion("subst") == true);


            var questionnaireRepository = new Mock<IQuestionnaireStorage>();
            questionnaireRepository.SetReturnsDefault(questionnaireMock);

            var answer = new TextAnswer();
            answer.SetAnswer("answer");
            var interview = Mock.Of<IStatefulInterview>(x => x.FindBaseAnswerByOrDeeperRosterLevel(substitutedQuesiton, Empty.RosterVector) == answer);

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetReturnsDefault(interview);
            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository.Object);
        };

        Because of = () => viewModel.Init("interview", new Identity(substitutionTargetId, Empty.RosterVector));

        It should_substitute_question_titles = () => viewModel.Title.HtmlText.ShouldEqual("title with answer");

        static QuestionHeaderViewModel viewModel;
        private static Guid substitutionTargetId;
        private static Guid substitutedQuesiton;
    }
}

