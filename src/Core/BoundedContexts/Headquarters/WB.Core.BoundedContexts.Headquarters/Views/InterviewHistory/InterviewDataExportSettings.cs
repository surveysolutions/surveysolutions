using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory
{
    public class InterviewDataExportSettings
    {
        public InterviewDataExportSettings()
        {
            this.EnableInterviewHistory = false;
            this.MaxRecordsCountPerOneExportQuery = 1000;
            this.LimitOfCachedItemsByDenormalizer = 100;
            this.InterviewsExportParallelTasksLimit = 1;
            this.DirectoryPath = String.Empty;
            this.ErrorsExporterBatchSize = 20;
        }

        public InterviewDataExportSettings(string directoryPath, 
            bool enableInterviewHistory, 
            string exportServiceUrl,
            int maxRecordsCountPerOneExportQuery = 1000, 
            int limitOfCachedItemsByDenormalizer = 100,
            int interviewsExportParallelTasksLimit = 1,
            int interviewIdsQueryBatchSize = 0,
            int errorsExporterBatchSize = 0)
        {
            this.EnableInterviewHistory = enableInterviewHistory;
            this.ExportServiceUrl = exportServiceUrl;
            this.MaxRecordsCountPerOneExportQuery = maxRecordsCountPerOneExportQuery;
            this.LimitOfCachedItemsByDenormalizer = limitOfCachedItemsByDenormalizer;
            this.InterviewsExportParallelTasksLimit = interviewsExportParallelTasksLimit;
            this.InterviewIdsQueryBatchSize = interviewIdsQueryBatchSize;
            this.ErrorsExporterBatchSize = errorsExporterBatchSize;
            this.DirectoryPath = directoryPath;
        }

        public int ErrorsExporterBatchSize { get; }

        public string DirectoryPath { get; private set; }

        public bool EnableInterviewHistory { get; private set; }
        public string ExportServiceUrl { get; }

        public int MaxRecordsCountPerOneExportQuery { get; private set; }

        public int InterviewsExportParallelTasksLimit { get; private set; }

        public int InterviewIdsQueryBatchSize { get; set; }

        public string ExportedDataFolderName
        {
            get { return "ExportedEventHistory"; }
        }

        public int LimitOfCachedItemsByDenormalizer { get; private set; }
    }
}
