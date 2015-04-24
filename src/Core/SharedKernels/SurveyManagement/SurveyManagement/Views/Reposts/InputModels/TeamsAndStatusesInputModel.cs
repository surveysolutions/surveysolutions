using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class TeamsAndStatusesInputModel : ListViewModelBase
    {
        public Guid? TemplateId { get; set; }

        public long? TemplateVersion { get; set; }

        public Guid? ViewerId { get; set; }
    }
}