using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.IntegerQuestionViewModelTests
{
    [TestOf(typeof(IntegerQuestionViewModel))]
    internal class when_answering_numeric_question_with_big_negative_value : IntegerQuestionViewModelTestContext
    {
        [SetUp]
        public void Context()
        {
            SetUp();

            var integerNumericAnswer = Mock.Of<InterviewTreeIntegerQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.NumericIntegerAnswer(1));

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetIntegerQuestion(questionIdentity) == integerNumericAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithNumericQuestion();

            integerModel = CreateIntegerQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            integerModel.Init(interviewId, questionIdentity, navigationState);
        
            integerModel.Answer = decimal.MinValue;
            integerModel.ValueChangeCommand.Execute();
        }

        [Test]
        public void should_mark_question_as_invalid_with_message () =>
            ValidityModelMock.Verify(x => x.MarkAnswerAsNotSavedWithMessage("Entered value cannot be parsed as integer value"), Times.Once);

        [Test]
        public void  should_not_send_answer_command () =>
            AnsweringViewModelMock.Verify(x => x.SendQuestionCommandAsync(Moq.It.IsAny<AnswerNumericIntegerQuestionCommand>()), Times.Never);

        private static IntegerQuestionViewModel integerModel;
    }
}
