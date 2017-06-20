using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Fetching;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    internal class AssignmentsService : IAssignmentsService
    {
        private readonly IPlainStorageAccessor<Assignment> assignmentsAccessor;

        public AssignmentsService(IPlainStorageAccessor<Assignment> assignmentsAccessor)
        {
            this.assignmentsAccessor = assignmentsAccessor;
        }

        public List<Assignment> GetAssignments(Guid responsibleId)
        {
            return this.assignmentsAccessor.Query(x =>
            x.Where(assigment =>
                assigment.ResponsibleId == responsibleId
                && !assigment.Archived
                && (assigment.Quantity == null || assigment.InterviewSummaries.Count < assigment.Quantity))
            .ToList());
        }

        public Assignment GetAssignment(int id)
        {
            return this.assignmentsAccessor.GetById(id);
        }

        public List<Assignment> GetAssignmentsReadyForWebInterview(QuestionnaireIdentity questionnaireId)
        {
            var assignmentsReadyForWebInterview = this.assignmentsAccessor.Query(_ => _
                .Where(x => x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                    x.QuestionnaireId.Version == questionnaireId.Version &&
                    x.Responsible.ReadonlyProfile.SupervisorId != null &&
                    !x.Archived)
                .OrderBy(x => x.Id)
                .Fetch(x => x.IdentifyingData)
                .ToList());
            return assignmentsReadyForWebInterview;
        }

        public int GetCountOfAssignmentsReadyForWebInterview(QuestionnaireIdentity questionnaireId)
        {
            return this.assignmentsAccessor.Query(_ => _.Count(
                x => x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                    x.QuestionnaireId.Version == questionnaireId.Version &&
                    x.Responsible.ReadonlyProfile.SupervisorId != null &&
                    !x.Archived));
        }
    }
}