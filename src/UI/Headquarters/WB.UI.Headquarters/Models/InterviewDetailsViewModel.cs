using System;

namespace WB.UI.Headquarters.Models
{
    public class InterviewDetailsViewModel
    {
        public Guid InterviewId { get; set; }
        public Guid? CurrentGroupId { get; set; }
        public Guid? CurrentPropagationKey { get; set; }
    }
}