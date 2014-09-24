using System;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewDataExportView : IView
    {
        public InterviewDataExportView(Guid interviewId, Guid templateId, long templateVersion, InterviewDataExportLevelView[] levels)
        {
            this.InterviewId = interviewId;
            this.TemplateId = templateId;
            this.TemplateVersion = templateVersion;
            this.Levels = levels;
        }
        public Guid InterviewId { get; private set; }
        public Guid TemplateId { get; private set; }
        public long TemplateVersion { get; private set; }
        public InterviewDataExportLevelView[] Levels { get; private set; }
    }
}
