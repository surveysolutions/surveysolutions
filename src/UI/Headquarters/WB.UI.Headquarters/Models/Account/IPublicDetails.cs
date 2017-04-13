namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public interface ICommonUserDetails
    {
        string Email { get; set; }
        string PersonName { get; set; }
        string PhoneNumber { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string ConfirmPassword { get; set; }
        bool IsLocked { get; set; }
    }
}