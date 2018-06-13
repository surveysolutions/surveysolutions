using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.IntegerQuestionViewModelTests
{
    internal class when_question_is_answered_with_special_value : IntegerQuestionViewModelTestContext
    {
        [Test]
        public void should_not_set_answer_value()
        {
            SetUp();

            var questionId = Id.g1;
            var entityIdentity = Create.Identity(questionId);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.NumericIntegerQuestion(
                id: questionId,
                specialValues: new[] {Create.Entity.Answer("spec1", -1)}
            ));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: questionId, answer: -1));

            var questionnaireStorage = Abc.Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var interviewRepository = Abc.Setup.StatefulInterviewRepository(interview);

            FilteredOptionsViewModel optionsViewModel = Create.ViewModel.FilteredOptionsViewModel(entityIdentity, questionnaire, interview);

            var viewModel = CreateIntegerQuestionViewModel(questionnaireStorage, interviewRepository, 
                specialValuesViewModel: Create.ViewModel.SpecialValues(optionsViewModel: optionsViewModel, interviewRepository: interviewRepository));

            // Act
            viewModel.Init(Id.g2.FormatGuid(), entityIdentity, navigationState);

            // Assert
            Assert.That(viewModel.Answer, Is.Null);
        }
    }
}
