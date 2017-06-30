using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class ImportDataVerificationState
    {
        public ImportDataVerificationState()
        {
            this.WasResponsibleProvided = false;
            this.Errors = new List<PanelImportVerificationError>();
        }

        public List<PanelImportVerificationError> Errors { set; get; }

        public bool WasResponsibleProvided { set; get; }

        public int EntitiesCount { get; set; }

        public int EnumeratorsCount { get; set; }

        public int SupervisorsCount { get; set; }

        public string FileName { get; set; }
    }
}
