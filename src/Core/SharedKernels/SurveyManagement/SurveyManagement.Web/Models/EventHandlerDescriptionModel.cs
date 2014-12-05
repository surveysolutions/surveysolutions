namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class EventHandlerDescriptionModel
    {
        public string Name { get; set; }
        public bool SupportsPartialRebuild { get; set; }
        public string[] UsesViews { get; set; }
        public string[] BuildsViews { get; set; }
    }
}