using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_web_passwords_and_2_passwords_are_duplicated_should_return_2_PL0061_errors()
        {
            // arrange
            var password = "password";
            var fileName = "mainfile.tab";

            var preloadingRows = new List<PreloadingAssignmentRow>(new[]
            {
                Create.Entity.PreloadingAssignmentRow(fileName,
                    assignmentPassword: Create.Entity.AssignmentPassword(password),
                    quantity: Create.Entity.AssignmentQuantity(parsedQuantity: 1),
                    assignmentWebMode: Create.Entity.AssignmentWebMode(true)),

                Create.Entity.PreloadingAssignmentRow(fileName,
                    assignmentPassword: Create.Entity.AssignmentPassword(password),
                    quantity: Create.Entity.AssignmentQuantity(parsedQuantity: 1),
                    assignmentWebMode: Create.Entity.AssignmentWebMode(null))
            });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyWebPasswords(preloadingRows, null).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));
            Assert.That(errors.Select(x => x.Code), Has.All.EqualTo("PL0061"));
            Assert.That(errors.Select(x => x.References.First().Content), Has.All.EqualTo(password));
            Assert.That(errors.Select(x => x.References.First().DataFile), Has.All.EqualTo(fileName));
        }

        [Test]
        public void when_verify_web_passwords_and_password_has_duplicate_in_file_and_quantity_is_null_should_return_1_PL0061_error()
        {
            // arrange
            var password = "password";
            var fileName = "mainfile.tab";

            var preloadingRows = new List<PreloadingAssignmentRow>(new[]
            {
                Create.Entity.PreloadingAssignmentRow(fileName,
                    assignmentPassword: Create.Entity.AssignmentPassword(password),
                    quantity: Create.Entity.AssignmentQuantity(parsedQuantity: null),
                    assignmentWebMode: Create.Entity.AssignmentWebMode(true)),
                Create.Entity.PreloadingAssignmentRow(fileName,
                    assignmentPassword: Create.Entity.AssignmentPassword(password),
                    quantity: Create.Entity.AssignmentQuantity(parsedQuantity: null),
                    assignmentWebMode: Create.Entity.AssignmentWebMode(true))
            });

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneQuestion());

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyWebPasswords(preloadingRows, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));
            Assert.That(errors[0].Code, Is.EqualTo("PL0061"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(password));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
            
            Assert.That(errors[1].Code, Is.EqualTo("PL0061"));
            Assert.That(errors[1].References.First().Content, Is.EqualTo(password));
            Assert.That(errors[1].References.First().DataFile, Is.EqualTo(fileName));
        }
        
        [Test]
        public void when_verify_web_passwords_and_password_has_duplicate_in_db_should_return_1_PL0061_error()
        {
            // arrange
            var password = "password";
            var fileName = "mainfile.tab";
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var assignmentId = Guid.Parse("11111111111111111111111111111111");

            var preloadingRows = new List<PreloadingAssignmentRow>(new[]
            {
                Create.Entity.PreloadingAssignmentRow(fileName,
                    assignmentPassword: Create.Entity.AssignmentPassword(password),
                    quantity: Create.Entity.AssignmentQuantity(parsedQuantity: 1),
                    assignmentWebMode: Create.Entity.AssignmentWebMode(true))
            });

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneQuestion(
                    questionnaireId: questionnaireIdentity.QuestionnaireId), questionnaireIdentity.Version);

            var assignmentsRepository = Create.Storage.InMemoryReadSideStorage<Assignment, Guid>();
            assignmentsRepository.Store(new Assignment(assignmentId, 1, questionnaireIdentity, assignmentId, 1, false, "", password, true, null, null), assignmentId);

            var verifier = Create.Service.ImportDataVerifier(assignmentsRepository: assignmentsRepository);

            // act
            var errors = verifier.VerifyWebPasswords(preloadingRows, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0061"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(password));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_web_special_passwords_no_error_should_be_fired()
        {
            // arrange
            var password = "password";
            var fileName = "mainfile.tab";
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var assignmentId = Guid.Parse("11111111111111111111111111111111");

            var preloadingRows = new List<PreloadingAssignmentRow>(new[]
            {
                Create.Entity.PreloadingAssignmentRow(fileName,
                    assignmentEmail: Create.Entity.AssignmentEmail(null),
                    assignmentPassword: Create.Entity.AssignmentPassword(AssignmentConstants.PasswordSpecialValue),
                    quantity: Create.Entity.AssignmentQuantity(parsedQuantity: 1),
                    assignmentWebMode: Create.Entity.AssignmentWebMode(true)),
                Create.Entity.PreloadingAssignmentRow(fileName,
                    assignmentEmail: Create.Entity.AssignmentEmail(""),
                    assignmentPassword: Create.Entity.AssignmentPassword(AssignmentConstants.PasswordSpecialValue),
                    quantity: Create.Entity.AssignmentQuantity(parsedQuantity: 1),
                    assignmentWebMode: Create.Entity.AssignmentWebMode(true))
            });

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneQuestion(
                    questionnaireId: questionnaireIdentity.QuestionnaireId), questionnaireIdentity.Version);

            var assignmentsRepository = Create.Storage.InMemoryReadSideStorage<Assignment, Guid>();
            assignmentsRepository.Store(new Assignment(assignmentId, 1, questionnaireIdentity, assignmentId, 1, false, "", password, true, null, null), assignmentId);

            var verifier = Create.Service.ImportDataVerifier(assignmentsRepository: assignmentsRepository);

            // act
            var errors = verifier.VerifyWebPasswords(preloadingRows, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(0));
        }
    }
}
