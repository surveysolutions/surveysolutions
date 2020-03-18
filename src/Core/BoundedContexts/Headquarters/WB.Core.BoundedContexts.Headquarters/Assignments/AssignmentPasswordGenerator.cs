using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public sealed class AssignmentPasswordGenerator : RandomStringGenerator, IAssignmentPasswordGenerator
    {
        private readonly IQueryableReadSideRepositoryReader<Assignment, Guid> assignments;
        private readonly IPlainStorageAccessor<AssignmentToImport> importAssignments;

        public AssignmentPasswordGenerator(
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignments, 
            IPlainStorageAccessor<AssignmentToImport> importAssignments)
        {
            this.assignments = assignments;
            this.importAssignments = importAssignments;
        }

        public string GenerateUnique(int length)
        {
            var passwords = Enumerable.Range(1, 20).Select(_ => GetRandomString(length, Encode_32_Chars)).ToArray();

            List<string> usedPasswordsInAssignments = this.assignments.Query(_ => _.Where(x => passwords.Contains(x.Password)).Select(x => x.Password).ToList());
            List<string> usedPasswordsInAssignmentsToImport = this.importAssignments.Query(_ => _.Where(x => passwords.Contains(x.Password)).Select(x => x.Password).ToList());

            var availableTokens = passwords
                .Except(usedPasswordsInAssignments)
                .Except(usedPasswordsInAssignmentsToImport)
                .ToList();

            if (availableTokens.Any())
            {
                return availableTokens.First();
            }

            return GenerateUnique(length);
        }

        public string GetPassword(string password)
        {
            if (password == AssignmentConstants.PasswordSpecialValue)
                return this.GenerateUnique(6);
            return password;
        }
    }

    public interface IAssignmentPasswordGenerator
    {
        string GenerateUnique(int length);
        string GetPassword(string password);
    }
}
