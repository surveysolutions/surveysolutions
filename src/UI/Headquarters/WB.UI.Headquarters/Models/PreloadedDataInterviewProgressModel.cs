using System;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;

namespace WB.UI.Headquarters.Models
{
    public class PreloadedDataInterviewProgressModel
    {
        public AssignmentsImportStatus Status { get; set; }

        public Guid QuestionnaireId { get; set; }

        public long Version { get; set; }

        public string QuestionnaireTitle { get; set; }
    }
}
