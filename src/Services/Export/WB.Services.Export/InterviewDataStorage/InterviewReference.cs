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

        public DateTime? DeletedAtUtc { get; set; }
    }

    public class DeletedQuestionnaireReference
    {
        protected DeletedQuestionnaireReference()
        {
        }

        public DeletedQuestionnaireReference(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }
    }

    [Table("metadata")]
    public class Metadata
    {
        [Key]
        public string Id { get; set; }

        public string Value { get; set; }

        private long? value;

        [NotMapped]
        public long AsLong
        {
            get => value ?? (value = long.Parse(Value)) ?? 0;
            set
            {
                this.value = null;
                Value = value.ToString();
            }
        }
    }
}
