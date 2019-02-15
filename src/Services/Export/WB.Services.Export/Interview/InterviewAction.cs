﻿using System;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview.Entities;

namespace WB.Services.Export.Interview
{
    public class InterviewAction
    {
        public Guid InterviewId { get; set; }
        public string Key { get; set; }
        public InterviewExportedAction Status { get; set; }
        public string StatusChangeOriginatorName { get; set; }
        public UserRoles StatusChangeOriginatorRole { get; set; }
        public DateTime Timestamp { get; set; }
        public string SupervisorName { get; set; }
        public string InterviewerName { get; set; }
    }
}
