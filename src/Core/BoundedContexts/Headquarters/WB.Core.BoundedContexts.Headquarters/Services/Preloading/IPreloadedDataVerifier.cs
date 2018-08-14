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
        IEnumerable<PanelImportVerificationError> VerifyFiles(string originalFileName, PreloadedFileInfo[] files, IQuestionnaire questionnaire);
        IEnumerable<PanelImportVerificationError> VerifyFile(string fileName, PreloadedFileInfo file, IQuestionnaire questionnaire);

        IEnumerable<PanelImportVerificationError> VerifyProtectedVariables(string originalFileName, PreloadedFile file,
            IQuestionnaire questionnaire);

        InterviewImportError VerifyWithInterviewTree(IList<InterviewAnswer> answers, Guid? responsibleId, IQuestionnaire questionnaire);

    }
}
