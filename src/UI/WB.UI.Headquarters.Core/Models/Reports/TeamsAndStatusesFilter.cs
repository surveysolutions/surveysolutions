using System;
using WB.UI.Headquarters.Models.Api;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class TeamsAndStatusesFilter : DataTableRequest
    {
        public Guid? TemplateId { get; set; }
        public long? TemplateVersion { get; set; }
    }
}
