using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class InterviewDataExportView
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
