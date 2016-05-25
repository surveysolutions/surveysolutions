using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class RebuildReadSideInputViewModel
    {
        public string[] ListOfHandlers { get; set; }
        public int NumberOfSkipedEvents { get; set; }
        public Guid[] ListOfEventSources { get; set; }
        public RebuildReadSideType RebuildType { get; set; }
    }
}