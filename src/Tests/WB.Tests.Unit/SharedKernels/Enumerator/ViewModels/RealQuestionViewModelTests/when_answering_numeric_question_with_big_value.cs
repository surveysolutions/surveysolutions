using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RealQuestionViewModelTests
{
    internal class when_answering_numeric_question_with_big_value : RealQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetRealNumericAnswer(questionIdentity) == new Mock<RealNumericAnswer>().Object);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithNumericQuestion();

            model = CreateViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            model.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () =>
        {
            model.Answer = decimal.MaxValue;
            model.ValueChangeCommand.Execute();
        };

        It should_mark_question_as_invalid_with_message = () =>
            ValidityModelMock.Verify(x => x.MarkAnswerAsNotSavedWithMessage("Entered value can not be parsed as decimal value"), Times.Once);

        It should_not_send_answer_command = () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerNumericRealQuestionCommand>()), Times.Never);

        private static RealQuestionViewModel model;
    }
}