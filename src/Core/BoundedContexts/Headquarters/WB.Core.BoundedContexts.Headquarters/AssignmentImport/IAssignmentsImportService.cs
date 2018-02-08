using System.Collections.Generic;
using System.IO;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IAssignmentsImportService
    {
        PreloadedDataByFile ParseText(Stream inputStream, string fileName);
        IEnumerable<PreloadedDataByFile> ParseZip(Stream inputStream);
        IEnumerable<PreloadedFileMetaData> ParseZipMetadata(Stream inputStream);
    }
}