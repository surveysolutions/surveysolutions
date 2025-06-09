using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WB.Services.Export.InterviewDataStorage
{
    [Table("metadata")]
    public class Metadata
    {
        [Key] public required string Id { get; set; } 

        public required string Value { get; set; }

        private long? asLong;

        [NotMapped]
        public long AsLong
        {
            get => asLong ?? (long) (asLong = long.Parse(Value));
            set
            {
                this.asLong = null;
                Value = value.ToString();
            }
        }
    }
}
