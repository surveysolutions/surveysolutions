using System;
using System.Linq;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_answers_and_password_is_too_short_should_return_PL0056_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var invalidPassword = "AWE23";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, assignmentPassword: Create.Entity.AssignmentPassword(invalidPassword),
                assignmentWebMode: Create.Entity.AssignmentWebMode(true));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0056"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(invalidPassword));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }
        
        [Test]
        [TestCase("AWE232@")]
        [TestCase("      ")]
        public void when_verify_answers_and_password_contains_incorrect_characters_should_return_PL0056_error(string invalidPassword)
        {
            // arrange
            var fileName = "mainfile.tab";
            
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, assignmentPassword: Create.Entity.AssignmentPassword(invalidPassword),
                assignmentWebMode: Create.Entity.AssignmentWebMode(true));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0056"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(invalidPassword));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_email_is_not_valid_should_return_PL0055_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var invalidEmail = "email...";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, assignmentEmail :Create.Entity.AssignmentEmail(invalidEmail), 
                assignmentWebMode: Create.Entity.AssignmentWebMode(true));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0055"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(invalidEmail));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_email_is_not_empty_and_quantity_is_3_should_return_PL0057_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var validEmail = "email@example.com";
            var invalidQuantity = "3";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                quantity: Create.Entity.AssignmentQuantity(invalidQuantity, Int32.Parse(invalidQuantity)),
                assignmentEmail: Create.Entity.AssignmentEmail(validEmail),
                assignmentWebMode: Create.Entity.AssignmentWebMode(true));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0057"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(invalidQuantity));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_email_is_not_empty_and_web_mode_not_set_should_return_PL0057_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var validEmail = "email@example.com";
            var quantity = "1";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                quantity: Create.Entity.AssignmentQuantity(quantity, Int32.Parse(quantity)),
                assignmentEmail: Create.Entity.AssignmentEmail(validEmail));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0058"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(validEmail));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_web_mode_is_set_and_quantity_is_1_with_no_email_or_password_should_return_PL0060_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var quantity = "1";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                quantity: Create.Entity.AssignmentQuantity(quantity, Int32.Parse(quantity)),
                assignmentWebMode: Create.Entity.AssignmentWebMode(true));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0060"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(quantity));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_in_web_mode_with_nullable_quantity_email_and_password_should_return_PL0060_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var quantity = "1";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, assignmentWebMode: Create.Entity.AssignmentWebMode(true));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0060"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(quantity));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_in_web_mode_with_nullable_email_and_password_and_empty_quantity_should_return_PL0060_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var quantity = "1";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                assignmentWebMode: Create.Entity.AssignmentWebMode(true),
                quantity: Create.Entity.AssignmentQuantity(""));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0060"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(quantity));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }
    }
}
