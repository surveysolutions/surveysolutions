using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IPreloadedDataVerifier
    {
        IEnumerable<PanelImportVerificationError> VerifyAnswers(PreloadingAssignmentRow assignmentRow, IQuestionnaire questionnaire);
        IEnumerable<PanelImportVerificationError> VerifyColumns(PreloadedFileInfo[] files, IQuestionnaire questionnaire);
        IEnumerable<PanelImportVerificationError> VerifyRosters(List<PreloadingAssignmentRow> allRowsByAllFiles, IQuestionnaire questionnaire);
        IEnumerable<PanelImportVerificationError> VerifyFiles(PreloadedFileInfo[] files, IQuestionnaire questionnaire);
        InterviewImportError VerifyWithInterviewTree(IList<InterviewAnswer> answers, Guid? responsibleId, IQuestionnaire questionnaire);
    }
}
