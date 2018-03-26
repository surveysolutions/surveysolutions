using System.Collections.Generic;
using System.IO;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IAssignmentsImportService
    {
        PreloadedFile ParseText(Stream inputStream, string fileName);
        IEnumerable<PreloadedFile> ParseZip(Stream inputStream);
        IEnumerable<PreloadedFileMetaData> ParseZipMetadata(Stream inputStream);
    }
}