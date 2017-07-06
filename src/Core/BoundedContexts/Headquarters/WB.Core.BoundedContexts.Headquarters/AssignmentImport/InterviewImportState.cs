using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class InterviewImportState
    {
        public string Delimiter { get; private set; } = "\t";
        public string[] Columns { get; set; }
        public List<InterviewImportError> Errors { get; set; }
    }
}