using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public class InterviewExportedLevel
    {
        public InterviewExportedLevel() {}

        public InterviewExportedLevel(Guid scopeId, decimal[] rosterVector, ExportedQuestion[] questions)
        {
            this.ScopeId = scopeId;
            this.RosterVector = rosterVector;
            this.Questions = questions;
        }

        public Guid ScopeId { get; set; }
        public decimal[] RosterVector { get; set; }
        public ExportedQuestion[] Questions { get; set; }
    }
}
