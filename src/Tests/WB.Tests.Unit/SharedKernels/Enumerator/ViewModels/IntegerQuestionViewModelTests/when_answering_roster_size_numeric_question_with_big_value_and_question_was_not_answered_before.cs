using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.IntegerQuestionViewModelTests
{
    [TestOf(typeof(IntegerQuestionViewModel))]
    public class when_answering_roster_size_numeric_question_with_big_value_and_question_was_not_answered_before : IntegerQuestionViewModelTestContext
    {
        [Test]
        public async Task should_not_send_answer_command()
        {
            SetUp();

            var integerNumericAnswer = Mock.Of<InterviewTreeIntegerQuestion>(_ => _.IsAnswered() == false);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetIntegerQuestion(questionIdentity) == integerNumericAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithNumericQuestion();

            var integerModel = CreateIntegerQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            integerModel.Init(interviewId, questionIdentity, navigationState);

            integerModel.Answer = 70;
            // Act
            await integerModel.ValueChangeCommand.ExecuteAsync();

            // Assert
            ValidityModelMock.Verify(x => x.MarkAnswerAsNotSavedWithMessage("Answer '70' is incorrect because answer is greater than Roster upper bound '60'."),
                Times.Once);

            AnsweringViewModelMock.Verify(x => x.SendQuestionCommandAsync(It.IsAny<AnswerNumericIntegerQuestionCommand>()), Times.Never);

            integerModel.Answer.Should().Be(70);
        }

    }
}
