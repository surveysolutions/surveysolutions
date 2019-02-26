using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
    [Table("metadata")]
    public class Metadata
    {
        [Key]
        public string Id { get; set; }

        public string Value { get; set; }

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
