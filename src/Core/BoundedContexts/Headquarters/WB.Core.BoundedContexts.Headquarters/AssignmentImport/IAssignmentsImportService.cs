using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IAssignmentsImportService
    {
        IEnumerable<PanelImportVerificationError> VerifySimple(PreloadedFile file,
            IQuestionnaire questionnaire);

        IEnumerable<PanelImportVerificationError> VerifyPanel(string originalFileName, PreloadedFile[] allImportedFiles,
            IQuestionnaire questionnaire, List<string> protectedVariablesFileContent);

        void ImportAssignment(int assignmentId, IQuestionnaire questionnaire);

        AssignmentToImport GetAssignmentById(int assignmentId);
        int[] GetAllAssignmentIdsToVerify();
        int[] GetAllAssignmentIdsToImport();
        AssignmentsImportStatus GetImportStatus();
        void RemoveAllAssignmentsToImport();
        void SetResponsibleToAllImportedAssignments(Guid responsibleId);
        IEnumerable<string> GetImportAssignmentsErrors();
        void SetVerifiedToAssignment(int assignmentId, string errorMessage);
        void RemoveAssignmentToImport(int assignmentId);
        void SetImportProcessStatus(AssignmentsImportProcessStatus status);
    }
}
