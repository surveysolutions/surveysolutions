using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentViewFactory
    {
        AssignmentsWithoutIdentifingData Load(AssignmentsInputModel input);
        List<AssignmentIdentifyingQuestionRow> GetIdentifyingColumnText(Assignment assignment);
        Task<AssignmentHistory> LoadHistoryAsync(Guid assignmentPublicKey, int offset, int limit);
    }
}
