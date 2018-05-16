using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier
{
    static class Extensions
    {
        public static PreloadedFileInfo FindProtectedVariables(this IEnumerable<PreloadedFileInfo> files)
        {
            return files.FirstOrDefault(x => x.IsProtectedVariablesFile);
        }

        public static IEnumerable<PreloadedFileInfo> ExceptProtectedVariables(this IEnumerable<PreloadedFileInfo> files)
        {
            return files.Where(x => !x.IsProtectedVariablesFile);
        }
    }
}
