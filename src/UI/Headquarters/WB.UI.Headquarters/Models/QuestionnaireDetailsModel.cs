using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.UI.Headquarters.Models
{
    public class QuestionnaireDetailsModel
    {
        public string Title { get; set; }
        public long Version { get; set; }
        public DateTime ImportDateUtc { get; set; }
        public DateTime LastEntryDateUtc { get; set; }
        public DateTime CreationDateUtc { get; set; }
        public bool AudioAudit { get; set; }
        public bool WebMode { get; set; }
        public int SectionsCount { get; set; }
        public int SubSectionsCount { get; set; }
        public int RostersCount { get; set; }
        public int QuestionsCount { get; set; }
        public int QuestionsWithConditionsCount { get; set; }
        public string DesignerUrl { get; internal set; }
    }
}
