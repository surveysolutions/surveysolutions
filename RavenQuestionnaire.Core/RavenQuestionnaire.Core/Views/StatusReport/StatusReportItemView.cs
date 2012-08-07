using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class StatusReportItemView
    {
        public Guid StatusId { get; set; }

        public string StatusTitle { get; set; }

        public int QuestionnaireCount { get; set; }
        
    }
}
