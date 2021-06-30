using System;

namespace WB.Services.Export.Assignment
{
    public class Assignment
    {
        public int Id { get; set; }
        public Guid PublicKey { get; set; }
        public Guid ResponsibleId { get; set; }
        public int? Quantity { get; set; }
        public bool AudioRecording { get; set; }
        public bool? WebMode { get; set; }
        public string? Comment { get; set; }
        public string? QuestionnaireId { get; set; }
        
        public int? UpgradedFromId { get; set;}
    }
}
