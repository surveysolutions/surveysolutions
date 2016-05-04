using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
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
        }

        public InterviewDataExportSettings(string directoryPath, 
            bool enableInterviewHistory, 
            int maxRecordsCountPerOneExportQuery, 
            int limitOfCachedItemsByDenormalizer,
            int interviewsExportParallelTasksLimit,
            int interviewIdsQueryBatchSize)
        {
            this.EnableInterviewHistory = enableInterviewHistory;
            this.MaxRecordsCountPerOneExportQuery = maxRecordsCountPerOneExportQuery;
            this.LimitOfCachedItemsByDenormalizer = limitOfCachedItemsByDenormalizer;
            this.InterviewsExportParallelTasksLimit = interviewsExportParallelTasksLimit;
            this.InterviewIdsQueryBatchSize = interviewIdsQueryBatchSize;
            this.DirectoryPath = directoryPath;
        }

        public string DirectoryPath { get; private set; }

        public bool EnableInterviewHistory { get; private set; }

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
