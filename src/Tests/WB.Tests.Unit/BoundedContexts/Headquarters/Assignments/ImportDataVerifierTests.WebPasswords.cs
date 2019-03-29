using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
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
                    quantity: Create.Entity.AssignmentQuantity(parsedQuantity: 1))
            });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyWebPasswords(preloadingRows).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));
            Assert.That(errors.Select(x => x.Code), Has.All.EqualTo("PL0061"));
            Assert.That(errors.Select(x => x.References.First().Content), Has.All.EqualTo(password));
            Assert.That(errors.Select(x => x.References.First().DataFile), Has.All.EqualTo(fileName));
        }
    }
}
