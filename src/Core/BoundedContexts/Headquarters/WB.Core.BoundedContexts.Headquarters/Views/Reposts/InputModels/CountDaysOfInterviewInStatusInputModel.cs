using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels
{
    public class CountDaysOfInterviewInStatusInputModel
    {
        public Guid? TemplateId { get; set; }
        public long? TemplateVersion { get; set; }
    }
}