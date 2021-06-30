using System;
using WB.Services.Export.Events.Assignment.Base;

namespace WB.Services.Export.Events.Assignment
{
    public class AssignmentCreated : AssignmentEvent
    {
        public int Id { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public Guid ResponsibleId { get; set; }
        public int? Quantity { get; set; }
        public bool AudioRecording { get; set; }
        public bool? WebMode { get; set; }
        public string Comment { get; set; } = String.Empty;
        public int? UpgradedFromId { get; set;}
        
        public string QuestionnaireIdentity => QuestionnaireId.ToString("N") + "$" +
                                               QuestionnaireVersion;
    }
}
