using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public class InterviewHistoryWriterSettings
    {
        public InterviewHistoryWriterSettings(string directoryPath)
        {
            this.DirectoryPath = directoryPath;
        }

        public string DirectoryPath { get; private set; }

        public string ExportedDataFolderName
        {
            get { return "ExportedEventHistory"; }
        }
    }
}
