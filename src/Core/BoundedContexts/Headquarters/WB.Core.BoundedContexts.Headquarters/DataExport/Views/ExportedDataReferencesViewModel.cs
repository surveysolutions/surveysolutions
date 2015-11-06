using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class ExportedDataReferencesViewModel
    {
        public ExportedDataReferencesViewModel(
            Guid questionnaireId, 
            long questionnaireVersion, 
            ExportedDataReferencesView paradataReference,
            ExportedDataReferencesView tabularDataReference,
            ExportedDataReferencesView tabularApprovedDataReference,
            RunningDataExportProcessView[] runningProcesses)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.ParadataReference = paradataReference;
            this.TabularDataReference = tabularDataReference;
            this.TabularApprovedDataReference = tabularApprovedDataReference;
            this.RunningProcesses = runningProcesses;
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public ExportedDataReferencesView ParadataReference { get; private set; }
        public ExportedDataReferencesView TabularDataReference { get; private set; }
        public ExportedDataReferencesView TabularApprovedDataReference { get; private set; }
        public RunningDataExportProcessView[] RunningProcesses { get; private set; }
    }
}