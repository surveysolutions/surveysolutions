using System.Collections.Generic;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Services
{
    public class InterviewImportState
    {
        public string Delimiter { get; private set; } = "\t";
        public string[] Columns { get; set; }
        public List<InterviewImportError> Errors { get; set; }
    }
}