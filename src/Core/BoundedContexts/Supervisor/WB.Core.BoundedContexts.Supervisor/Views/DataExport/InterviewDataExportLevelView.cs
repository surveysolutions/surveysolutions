using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class InterviewDataExportLevelView
    {
        public InterviewDataExportLevelView(Guid levelId, string levelName, InterviewDataExportRecord[] records)
        {
            LevelId = levelId;
            LevelName = levelName;
            Records = records;
        }
        public Guid LevelId { get; private set; }
        public string LevelName { get; private set; }
        public InterviewDataExportRecord[] Records { get; set; }
    }
}
