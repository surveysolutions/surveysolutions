using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.UI.Headquarters.Models
{
    public class ExportModel
    {
        public IEnumerable<TemplateViewItem> Questionnaires { get; set; }
        public List<InterviewStatus> ExportStatuses { get; set; }
    }
}