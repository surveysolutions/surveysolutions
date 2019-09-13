using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    class AssignmentsToDeleteFactory : IAssignmentsToDeleteFactory
    {
        private readonly IQueryableReadSideRepositoryReader<Assignment> assignmentsReader;

        public const int BatchSize = 100;

        public AssignmentsToDeleteFactory(IQueryableReadSideRepositoryReader<Assignment> assignmentsReader)
        {
            this.assignmentsReader = assignmentsReader;
        }

        public List<Assignment> LoadBatch(Guid questionnaireId, long questionnaireVersion)
        {
            var result = this.assignmentsReader.Query(_ => _.Where(assignment =>
                    assignment.QuestionnaireId.QuestionnaireId == questionnaireId &&
                    assignment.QuestionnaireId.Version == questionnaireVersion)
                .Take(BatchSize)
                .ToList());

            return result;
        }
    }
}
