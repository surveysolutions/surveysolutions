using System.Collections.Generic;
using System.IO;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IAssignmentsImportService
    {
        IEnumerable<AssignmentRow> GetAssignmentRows(QuestionnaireIdentity questionnaireIdentity, PreloadedFile file);
        PreloadedFile ParseText(Stream inputStream, string fileName);
        IEnumerable<PreloadedFile> ParseZip(Stream inputStream);
    }
}
