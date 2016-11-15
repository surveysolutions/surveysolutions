using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    [Ignore("KP-8159")]
    internal class when_roster_title_substitution_is_used : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            substitutionTargetQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaireMock = Mock.Of<IQuestionnaire>(_
               => _.GetQuestionTitle(substitutionTargetQuestionId) == "uses %rostertitle%"
               && _.GetQuestionInstruction(substitutionTargetQuestionId) == "Instruction"
               );

            var interview = Mock.Of<IStatefulInterview>();

            //rosterTitleAnswerValue = "answer";
            //var rosterTitleSubstitutionService = new Mock<IRosterTitleSubstitutionService>();
            //rosterTitleSubstitutionService.Setup(x => x.Substitute(Moq.It.IsAny<string>(), Moq.It.IsAny<Identity>(), Moq.It.IsAny<string>()))
            //    .Returns<string, Identity, string>((title, id, interviewId) => title.Replace("%rostertitle%", rosterTitleAnswerValue));

            var questionnaireRepository = new Mock<IQuestionnaireStorage>();
            questionnaireRepository.SetReturnsDefault(questionnaireMock);

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetReturnsDefault(interview);

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository.Object);
        };

        Because of = () => 
            viewModel.Init("interview", new Identity(substitutionTargetQuestionId, Create.Entity.RosterVector(1)));

        It should_substitute_roster_title_value = () => 
            viewModel.Title.HtmlText.ShouldEqual($"uses {rosterTitleAnswerValue}");

        static QuestionHeaderViewModel viewModel;
        static Guid substitutionTargetQuestionId;
        static string rosterTitleAnswerValue;
    }
}