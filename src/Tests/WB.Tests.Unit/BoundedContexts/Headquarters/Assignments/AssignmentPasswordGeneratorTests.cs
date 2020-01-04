using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.Infrastructure.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(AssignmentPasswordGenerator))]
    class AssignmentPasswordGeneratorTests
    {
        [Test]
        public void When_generating_unique_one_letter_password()
        {
            var assignments = new InMemoryReadSideRepositoryAccessor<Assignment, Guid>();
            var importAssignments = new InMemoryPlainStorageAccessor<AssignmentToImport>();
            for (int i = 0; i < RandomStringGenerator.Encode_32_Chars.Length-1; i++)
            {
                var p = RandomStringGenerator.Encode_32_Chars.Substring(i, 1);
                if (i % 2 == 0)
                {
                    var assignment = Create.Entity.Assignment(id: i + 1, password:  p);
                    assignments.Store(assignment, assignment.PublicKey);
                }
                else
                {
                    var assignmentToImport = Create.Entity.AssignmentToImport(id: 100 + i, password:  p);
                    importAssignments.Store(assignmentToImport, assignmentToImport.Id);
                }
            }

            var generator = Create.Service.AssignmentPasswordGenerator(assignments: assignments, importAssignments: importAssignments);

            var password = generator.GenerateUnique(1);

            Assert.That(password, Is.EqualTo("Z"));
        }
    }
}
