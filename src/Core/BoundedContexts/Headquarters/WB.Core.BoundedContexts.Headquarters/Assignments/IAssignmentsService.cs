using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentsService
    {
        List<Assignment> GetAssignments(Guid responsibleId);
        Assignment GetAssignment(int id);

        List<Assignment> GetAssignmentsReadyForWebInterview(QuestionnaireIdentity questionnaireId);
        int GetCountOfAssignmentsReadyForWebInterview(QuestionnaireIdentity questionnaireId);
    }
}