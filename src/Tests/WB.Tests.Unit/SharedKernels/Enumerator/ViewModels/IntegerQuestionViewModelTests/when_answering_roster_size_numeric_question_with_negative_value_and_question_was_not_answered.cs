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
    internal class when_answering_roster_size_numeric_question_with_negative_value_and_question_was_not_answered : IntegerQuestionViewModelTestContext
    {
        [SetUp]
        public async Task Context()
        {
            SetUp();

            var integerNumericAnswer = Mock.Of<InterviewTreeIntegerQuestion>(_ => _.IsAnswered() == false);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetIntegerQuestion(questionIdentity) == integerNumericAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithNumericQuestion();

            integerModel = CreateIntegerQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            integerModel.Init(interviewId, questionIdentity, navigationState);
        
            integerModel.Answer = -4;
            await integerModel.ValueChangeCommand.ExecuteAsync();
        }

        [Test]
        public void  should_mark_question_as_invalid_with_message() =>
            ValidityModelMock.Verify(x => x.MarkAnswerAsNotSavedWithMessage("Answer '-4' is incorrect because question is a roster source question and specified answer is negative"), Times.Once);

        [Test]
        public void  should_not_send_answer_command() =>
            AnsweringViewModelMock.Verify(x => x.SendQuestionCommandAsync(Moq.It.IsAny<AnswerNumericIntegerQuestionCommand>()), Times.Never);

        private static IntegerQuestionViewModel integerModel;
    }
}
