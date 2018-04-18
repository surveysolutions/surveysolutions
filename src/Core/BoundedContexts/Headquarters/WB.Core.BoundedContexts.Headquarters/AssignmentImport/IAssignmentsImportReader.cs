using System.Collections.Generic;
using System.IO;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IAssignmentsImportReader
    {
        PreloadedFile ReadTextFile(Stream inputStream, string fileName);
        IEnumerable<PreloadedFile> ReadZipFile(Stream inputStream);
    }
}
