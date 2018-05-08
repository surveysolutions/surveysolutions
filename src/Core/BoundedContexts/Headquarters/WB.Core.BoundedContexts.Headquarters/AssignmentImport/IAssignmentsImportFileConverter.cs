using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IAssignmentsImportFileConverter
    {
        IEnumerable<PreloadingAssignmentRow> GetAssignmentRows(PreloadedFile file,
            IQuestionnaire questionnaire);
    }
}
