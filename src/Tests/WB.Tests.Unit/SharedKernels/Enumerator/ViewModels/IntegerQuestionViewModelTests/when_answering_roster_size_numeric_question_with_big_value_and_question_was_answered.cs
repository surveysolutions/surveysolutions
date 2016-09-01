using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.IntegerQuestionViewModelTests
{
    internal class when_answering_roster_size_numeric_question_with_big_value_and_question_was_answered : IntegerQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();

            var integerNumericAnswer = Mock.Of<IntegerNumericAnswer>(_ => _.IsAnswered == true && _.Answer == 1);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetIntegerNumericAnswer(questionIdentity) == integerNumericAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithNumericQuestion();

            integerModel = CreateIntegerQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            integerModel.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () =>
        {
            integerModel.Answer = 70;
            integerModel.ValueChangeCommand.Execute();
        };

        It should_mark_question_as_invalid_with_message = () =>
            ValidityModelMock.Verify(x => x.MarkAnswerAsNotSavedWithMessage("Answer '70' is incorrect because answer is greater than Roster upper bound '60'."), Times.Once);

        It should_not_send_answer_command = () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerNumericIntegerQuestionCommand>()), Times.Never);

        private static IntegerQuestionViewModel integerModel;
    }
}