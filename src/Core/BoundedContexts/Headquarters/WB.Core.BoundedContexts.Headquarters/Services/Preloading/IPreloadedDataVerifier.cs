using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IPreloadedDataVerifier
    {
        IEnumerable<PanelImportVerificationError> VerifyAnswers(AssignmentRow assignmentRow, IQuestionnaire questionnaire);
        IEnumerable<PanelImportVerificationError> VerifyColumns(PreloadedFileInfo[] files, IQuestionnaire questionnaire);
        IEnumerable<PanelImportVerificationError> VerifyRosters(List<AssignmentRow> allRowsByAllFiles, IQuestionnaire questionnaire);
        IEnumerable<PanelImportVerificationError> VerifyFile(PreloadedFileInfo file, IQuestionnaire questionnaire);
    }
}
