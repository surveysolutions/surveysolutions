using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WB.Services.Export.Interview;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewReference
    {
        public string QuestionnaireId { get; set; }

        public Guid InterviewId { get; set; }

        public InterviewStatus Status { get; set; }

        public string Key { get; set; }

        public DateTime? UpdateDateUtc { get; set; }
    }

    [Table("metadata")]
    public class Metadata
    {
        [Key]
        public string Id { get; set; }
        public long GlobalSequence { get; set; }
    }
}
