using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewQuestionnaireReferenceNode
    {
        public string QuestionnaireId { get; set; }
        public Guid InterviewId { get; set; }
    }

    [Table("metadata")]
    public class Metadata
    {
        [Key]
        public string Id { get; set; }
        public long GlobalSequence { get; set; }
    }
}
