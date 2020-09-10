using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentsToDeleteFactory
    {
        void RemoveAllAssignments(QuestionnaireIdentity questionnaireIdentity);
        void RemoveAllEventsForAssignments(QuestionnaireIdentity questionnaireIdentity);
    }
}
