using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WB.Services.Export.InterviewDataStorage;

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
        public string Comment { get; set; }
    }
}
