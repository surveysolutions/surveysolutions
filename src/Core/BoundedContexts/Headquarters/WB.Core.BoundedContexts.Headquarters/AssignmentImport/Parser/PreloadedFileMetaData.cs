using System.Diagnostics;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    [DebuggerDisplay("{FileName}")]
    public class PreloadedFileMetaData
    {
        public PreloadedFileMetaData(string fileName, long fileSize, bool canBeHandled)
        {
            this.CanBeHandled = canBeHandled;
            this.FileName = fileName;
            this.FileSize = fileSize;
        }

        public string FileName { get; private set; }
        public long FileSize { get; private set; }
        public bool CanBeHandled { get; private set; }
    }
}
