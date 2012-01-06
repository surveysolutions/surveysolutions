using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Documents
{
    public class ReportDocument
    {
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string BuildCondition { set; get; }
    }
}
