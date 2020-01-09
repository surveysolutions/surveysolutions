﻿using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class RunningDataExportProcessView
    {
        public RunningDataExportProcessView(string questionnaireId)
        { 
            this.QuestionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);
        }

        public long ProcessId { get; set; }

        public DateTime BeginDate { get;  set; }
        public DateTime LastUpdateDate { get;  set; }
        public int Progress { get;  set; }
        public DataExportType Type { get;  set; }
        public DataExportFormat Format { get;  set; }
        public InterviewStatus? InterviewStatus { get;  set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public DataExportStatus ProcessStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class DataExportProcessView
    {
        public DataExportProcessView(string questionnaireId)
        { 
            this.QuestionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);
        }

        public long Id { get; set; }

        public DateTime BeginDate { get;  set; }
        public DateTime LastUpdateDate { get;  set; }
        public int Progress { get;  set; }
        public DataExportType Type { get;  set; }
        public DataExportFormat Format { get;  set; }
        public InterviewStatus? InterviewStatus { get;  set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public DataExportStatus ProcessStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsRunning { get; set; }
        public TimeSpan? TimeEstimation { get; set; }
        public DataExportErrorView Error { get; set; }
        public bool HasFile { get; set; }
        public double FileSize { get; set; }
        public DateTime DataFileLastUpdateDate { get; set; }
        public string DataDestination { get; set; }
        public string Title { get; set; }
    }
}
