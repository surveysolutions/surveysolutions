using System;
using System.Collections.Generic;
using System.IO;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IAssignmentsImportService
    {
        PreloadedFile ParseText(Stream inputStream, string fileName);
        IEnumerable<PreloadedFile> ParseZip(Stream inputStream);

        IEnumerable<PanelImportVerificationError> VerifySimple(PreloadedFile file,
            QuestionnaireIdentity questionnaireIdentity);

        IEnumerable<PanelImportVerificationError> VerifyPanel(string originalFileName, PreloadedFile[] allImportedFiles,
            QuestionnaireIdentity questionnaireIdentity);

        AssignmentToImport GetAssignmentToImport();
        AssignmentToImport GetAssignmentToVerify();
        void RemoveImportedAssignment(int assignmentId);
        AssignmentsImportStatus GetImportStatus();
        void RemoveAllAssignmentsToImport();
        void SetResponsibleToAllImportedAssignments(Guid responsibleId);
        IEnumerable<string> GetImportAssignmentsErrors();

        void VerifyAssignment(AssignmentToImport assignment, QuestionnaireIdentity questionnaireIdentity);
        void ImportAssignment(AssignmentToImport assignmentToImport, QuestionnaireIdentity questionnaireIdentity);
    }
}
