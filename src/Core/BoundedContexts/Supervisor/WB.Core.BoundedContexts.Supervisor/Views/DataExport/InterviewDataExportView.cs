using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class InterviewDataExportView
    {
        public InterviewDataExportView(Guid templateId, long templateVersion, Guid? levelId, string levelName, ExportedHeaderCollection header, InterviewDataExportRerord[] records)
        {
            TemplateId = templateId;
            TemplateVersion = templateVersion;
            LevelId = levelId;
            LevelName = levelName;
            Header = header;
            Records = records;
        }

        public Guid TemplateId { get; private set; }
        public long TemplateVersion { get; private set; }
        public Guid? LevelId { get; private set; }
        public string LevelName { get; private set; }
        public ExportedHeaderCollection Header { get; private set; }
        public InterviewDataExportRerord[] Records { get; set; }
    }
}
