using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public class InterviewDataExportSettings
    {
        public InterviewDataExportSettings(string directoryPath, bool enableInterviewHistory, int maxRecordsCountPerOneExportQuery, int limitOfCachedItemsByDenormalizer)
        {
            this.EnableInterviewHistory = enableInterviewHistory;
            this.MaxRecordsCountPerOneExportQuery = maxRecordsCountPerOneExportQuery;
            this.LimitOfCachedItemsByDenormalizer = limitOfCachedItemsByDenormalizer;
            this.DirectoryPath = directoryPath;
        }

        public string DirectoryPath { get; private set; }

        public bool EnableInterviewHistory { get; private set; }

        public int MaxRecordsCountPerOneExportQuery { get; private set; }

        public string ExportedDataFolderName
        {
            get { return "ExportedEventHistory"; }
        }

        public int LimitOfCachedItemsByDenormalizer { get; private set; }
    }
}
