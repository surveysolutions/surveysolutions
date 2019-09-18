using System;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview.Entities;

namespace WB.Services.Export.Assignment
{
    public class AssignmentAction
    {
        public int Id { get; set; }
        public AssignmentExportedAction Status { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid OriginatorId { get; set; }
        public Guid ResponsibleId { get; set; }
    }
}
