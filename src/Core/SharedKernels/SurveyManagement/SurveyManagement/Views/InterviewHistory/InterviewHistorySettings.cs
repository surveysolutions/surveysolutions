using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public class InterviewHistorySettings
    {
        public InterviewHistorySettings(string directoryPath, bool enableInterviewHistory)
        {
            this.EnableInterviewHistory = enableInterviewHistory;
            this.DirectoryPath = directoryPath;
        }

        public string DirectoryPath { get; private set; }

        public bool EnableInterviewHistory { get; private set; }

        public string ExportedDataFolderName
        {
            get { return "ExportedEventHistory"; }
        }
    }
}
