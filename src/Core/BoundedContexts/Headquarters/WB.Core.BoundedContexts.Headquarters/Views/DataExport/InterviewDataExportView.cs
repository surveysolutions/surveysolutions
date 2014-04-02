using System;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public class InterviewDataExportView : IView
    {
        public InterviewDataExportView(Guid templateId, long templateVersion, InterviewDataExportLevelView[] levels)
        {
            this.TemplateId = templateId;
            this.TemplateVersion = templateVersion;
            this.Levels = levels;
        }

        public Guid TemplateId { get; private set; }
        public long TemplateVersion { get; private set; }
        public InterviewDataExportLevelView[] Levels { get; private set; }
    }
}
