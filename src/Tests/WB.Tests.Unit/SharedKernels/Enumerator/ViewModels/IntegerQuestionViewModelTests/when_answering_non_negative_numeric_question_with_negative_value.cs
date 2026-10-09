using System.Threading.Tasks;
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
    internal class when_answering_non_negative_numeric_question_with_negative_value : IntegerQuestionViewModelTestContext
    {
        [SetUp]
        public async Task Context()
        {
            SetUp();

            var integerQuestion = Mock.Of<InterviewTreeIntegerQuestion>(_ =>
                _.IsAnswered() == false);
            var interview = Mock.Of<IStatefulInterview>(_ =>
                _.QuestionnaireId == questionnaireId &&
                _.GetIntegerQuestion(questionIdentity) == integerQuestion);
            var viewModel = CreateIntegerQuestionViewModel(
                questionnaireRepository: SetupQuestionnaireRepositoryWithNumericQuestion(
                    isRosterSize: false, isNonNegative: true),
                interviewRepository: Mock.Of<IStatefulInterviewRepository>(
                    x => x.Get(interviewId) == interview));

            viewModel.Init(interviewId, questionIdentity, navigationState);
            viewModel.Answer = -4;
            await viewModel.ValueChangeCommand.ExecuteAsync();
        }

        [Test]
        public void should_mark_question_as_invalid()
            => ValidityModelMock.Verify(
                x => x.MarkAnswerAsNotSavedWithMessage("Negative values are not allowed for this question"),
                Times.Once);

        [Test]
        public void should_not_send_answer_command()
            => AnsweringViewModelMock.Verify(
                x => x.SendQuestionCommandAsync(Moq.It.IsAny<AnswerNumericIntegerQuestionCommand>()),
                Times.Never);
    }
}
