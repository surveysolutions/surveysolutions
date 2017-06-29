using System;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;

namespace WB.UI.Headquarters.Models
{
    public class PreloadedDataInterviewProgressModel
    {
        public AssignmentImportStatus Status { get; set; }

        public Guid QuestionnaireId { get; set; }

        public long Version { get; set; }

        public string QuestionnaireTitle { get; set; }
    }
}