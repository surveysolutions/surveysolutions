namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UserModelBase
    {
        public bool IsLockedDisabled { get; set; } = false;
        public bool HideDetails { get; set; } = false;
        public string CancelAction { get; set; } = @"Cancel";
        public object CancelArg { get; set; } = null;
        public string EditAction { get; set; } = "Edit";
    }
}