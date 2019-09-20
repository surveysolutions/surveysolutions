using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview.Entities;

namespace WB.Services.Export.Assignment
{
    public class AssignmentAction
    {
        public long SequenceIndex { get; set; }
        public int AssignmentId { get; set; }
        public AssignmentExportedAction Status { get; set; }
        public DateTime TimestampUtc { get; set; }
        public Guid OriginatorId { get; set; }
        public Guid ResponsibleId { get; set; }

        public Assignment Assignment { get; set; }
    }
}
