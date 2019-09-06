using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentViewFactory
    {
        AssignmentsWithoutIdentifingData Load(AssignmentsInputModel input);
        List<AssignmentIdentifyingQuestionRow> GetIdentifyingColumnText(Assignment assignment);
    }
}
