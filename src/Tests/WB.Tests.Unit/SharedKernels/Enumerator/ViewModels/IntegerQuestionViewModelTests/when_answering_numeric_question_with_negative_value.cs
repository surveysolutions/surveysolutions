using System.Threading;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.IntegerQuestionViewModelTests
{
    [TestOf(typeof(IntegerQuestionViewModel))]
    internal class when_answering_numeric_question_with_negative_value : IntegerQuestionViewModelTestContext
    {
        [SetUp]
        public void Context()
        {
            SetUp();

            var integerNumericAnswer = Mock.Of<InterviewTreeIntegerQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.NumericIntegerAnswer(3));

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetIntegerQuestion(questionIdentity) == integerNumericAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewGuid.FormatGuid()) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithNumericQuestion(isRosterSize: false);

            integerModel = CreateIntegerQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            integerModel.Init(interviewGuid.FormatGuid(), questionIdentity, navigationState);
       
            integerModel.Answer = -4;
            integerModel.ValueChangeCommand.Execute();
        }

        [Test]
        public void  should_not_mark_question_as_invalid_with_message () =>
            ValidityModelMock.Verify(x => x.MarkAnswerAsNotSavedWithMessage(Moq.It.IsAny<string>()), Times.Never);

        [Test]
        public void  should_send_answer_command () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerNumericIntegerQuestionCommand>()), Times.Once);


        private static IntegerQuestionViewModel integerModel;
    }
}
