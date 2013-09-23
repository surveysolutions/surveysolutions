using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class InterviewDataExportInputModel
    {
        public InterviewDataExportInputModel(Guid templateId, long templateVersion, Guid? levelId)
        {
            TemplateId = templateId;
            TemplateVersion = templateVersion;
            LevelId = levelId;
        }

        public Guid TemplateId { get; private set; }
        public long TemplateVersion { get; private set; }
        public Guid? LevelId { get; private set; }
    }
}
