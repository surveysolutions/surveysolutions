using System.Linq;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_answers_and_quantity_is_not_integer_should_return_PL0035_error()
        {
            // arrange
            string quantity = "not integer quantity";

            var fileName = "mainfile.tab";
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, quantity: Create.Entity.AssignmentQuantity(quantity));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0035"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(quantity));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_quantity_is_negative_integer_should_return_PL0036_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, quantity: Create.Entity.AssignmentQuantity(parsedQuantity: -2));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0036"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo("-2"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_quantity_is_infinity_should_return_empty_errors()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, quantity: Create.Entity.AssignmentQuantity(parsedQuantity: -1));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_answers_and_interview_id_is_empty_should_return_PL0042_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, interviewId: Create.Entity.AssignmentInterviewId(""));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0042"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(""));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }
    }
}
