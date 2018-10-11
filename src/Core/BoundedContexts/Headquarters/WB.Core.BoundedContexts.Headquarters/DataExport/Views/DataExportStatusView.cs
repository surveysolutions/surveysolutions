using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class DataExportStatusView
    {
        public DataExportStatusView(
            string questionnaireId,
            DataExportView[] dataExports,
            RunningDataExportProcessView[] runningDataExportProcesses)
        {
            this.DataExports = dataExports;
            this.RunningDataExportProcesses = runningDataExportProcesses;
            this.QuestionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);
        }

        QuestionnaireIdentity QuestionnaireIdentity { get; set; }

        public DataExportView[] DataExports { get; set; }
        public RunningDataExportProcessView[] RunningDataExportProcesses { get; set; }
    }
}
