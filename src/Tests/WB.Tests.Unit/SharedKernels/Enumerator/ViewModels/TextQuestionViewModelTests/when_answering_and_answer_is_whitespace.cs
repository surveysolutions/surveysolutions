using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextQuestionViewModelTests
{
    internal class when_answering_and_answer_is_whitespace : TextQuestionViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            SetUp();
            
            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetTextQuestion(questionIdentity) == Create.Entity.InterviewTreeTextQuestion(""));

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithNumericQuestion();

            model = CreateTextQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            model.Init(interviewId, questionIdentity, navigationState);
            BecauseOf();
        }

        public void BecauseOf() 
        {
            model.ValueChangeCommand.Execute(" ");
        }

        [NUnit.Framework.Test] public void should_mark_question_as_invalid_with_message () =>
            ValidityModelMock.Verify(x => x.MarkAnswerAsNotSavedWithMessage("Answer should not be empty"), Times.Once);

        [NUnit.Framework.Test] public void should_not_send_answer_command () =>
            AnsweringViewModelMock.Verify(x => x.SendQuestionCommandAsync(Moq.It.IsAny<AnswerTextQuestionCommand>()), Times.Never);

        private static TextQuestionViewModel model;
    }
}
