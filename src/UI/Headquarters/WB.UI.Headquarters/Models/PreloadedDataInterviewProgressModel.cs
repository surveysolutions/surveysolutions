using System;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Models
{
    public class PreloadedDataInterviewProgressModel
    {
        public InterviewImportStatus Status { get; set; }

        public Guid QuestionnaireId { get; set; }

        public long Version { get; set; }

        public string QuestionnaireTitle { get; set; }
    }
}