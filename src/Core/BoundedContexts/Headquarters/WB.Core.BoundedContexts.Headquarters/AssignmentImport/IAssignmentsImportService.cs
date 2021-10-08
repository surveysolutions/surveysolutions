using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IAssignmentsImportService
    {
        IEnumerable<PanelImportVerificationError> VerifySimpleAndSaveIfNoErrors(PreloadedFile file,
            Guid defaultResponsibleId, IQuestionnaire questionnaire);

        IEnumerable<PanelImportVerificationError> VerifyPanelAndSaveIfNoErrors(string originalFileName, PreloadedFile[] allImportedFiles,
            Guid defaultResponsibleId, PreloadedFile protectedVariablesFile, IQuestionnaire questionnaire);

        int ImportAssignment(int assignmentId, Guid defaultAssignedTo, IQuestionnaire questionnaire, Guid responsibleId);
        int ImportAssignment(AssignmentToImport assignment, IQuestionnaire questionnaire, Guid responsibleId);
        AssignmentToImport GetAssignmentById(int assignmentId);
        int[] GetAllAssignmentIdsToVerify();
        int[] GetAllAssignmentIdsToImport();
        AssignmentsImportStatus GetImportStatus();
        IEnumerable<string> GetImportAssignmentsErrors();
        void SetVerifiedToAssignment(int assignmentId, string errorMessage = null);
        void RemoveAssignmentToImport(int assignmentId);
        void SetImportProcessStatus(AssignmentsImportProcessStatus status);

        AssignmentToImport ConvertToAssignmentToImport(List<PreloadingAssignmentRow> assignment,
            IQuestionnaire questionnaire, List<string> protectedQuestions);
    }
}
