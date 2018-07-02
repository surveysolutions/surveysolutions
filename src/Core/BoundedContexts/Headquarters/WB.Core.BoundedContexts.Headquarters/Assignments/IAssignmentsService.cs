using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentsService
    {
        List<Assignment> GetAssignments(Guid responsibleId);

        List<Assignment> GetAssignmentsForSupervisor(Guid supervisorId);

        List<int> GetAllAssignmentIds(Guid responsibleId);
        
        Assignment GetAssignment(int id);

        List<Assignment> GetAssignmentsReadyForWebInterview(QuestionnaireIdentity questionnaireId);
        
        int GetCountOfAssignmentsReadyForWebInterview(QuestionnaireIdentity questionnaireId);

        AssignmentApiDocument MapAssignment(Assignment assignment);

        void Reassign(int assignmentId, Guid responsibleId);

        bool HasAssignmentWithProtectedVariables(Guid responsibleId);
    }
}
