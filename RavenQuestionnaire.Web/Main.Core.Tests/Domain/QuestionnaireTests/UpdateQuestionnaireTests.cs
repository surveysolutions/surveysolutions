using Main.Core.Domain;
using Main.Core.Events.Questionnaire;
using NUnit.Framework;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    [TestFixture]
    public class UpdateQuestionnaireTests : QuestionnaireARTestContext
    {
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void UpdateQuestionnaire_When_questionnaire_title_is_empty_or_contains_whitespaces_only_Then_throws_DomainException_with_type_QuestionnaireTitleRequired(string emptyTitle)
        {
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            // act
            TestDelegate act = () => questionnaire.UpdateQuestionnaire(emptyTitle);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionnaireTitleRequired));
        }

        [Test]
        public void UpdateQuestionnaire_When_questionnaire_title_is_not_empty_Then_raised_QuestionnaireUpdated_event_contains_questionnaire_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var nonEmptyTitle = "Title";
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();

                // act
                questionnaire.UpdateQuestionnaire(nonEmptyTitle);

                // assert
                Assert.That(GetSingleEvent<QuestionnaireUpdated>(eventContext).Title, Is.EqualTo(nonEmptyTitle));
            }
        }
    }
}