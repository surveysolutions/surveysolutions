using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.Models.DataExport
{
    public class ExportModel
    {
        public IEnumerable<TemplateViewItem> Questionnaires { get; set; }
        public List<InterviewStatus> ExportStatuses { get; set; }
        public ExternalStoragesSettings ExternalStoragesSettings { get; set; }
    }

    public class NewExportModel
    {
        public dynamic Api { get; set; }
        public ComboboxViewItem[] Statuses { get; set; }
        public ExternalStoragesSettings ExternalStoragesSettings { get; set; }
    }
}
