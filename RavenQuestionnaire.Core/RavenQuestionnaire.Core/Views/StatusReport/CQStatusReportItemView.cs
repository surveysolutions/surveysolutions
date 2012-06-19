using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class CQStatusReportItemView
    {
        public string Description { get; set; }

        public UserLight AssignToUser { get; set; }

        public DateTime LastSyncDate { get; set; }

        public DateTime LastChangeDate { get; set; }
    }
}

