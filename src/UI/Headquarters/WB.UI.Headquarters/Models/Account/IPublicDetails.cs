namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public interface IPublicDetails
    {
        string Email { get; set; }
        string PersonName { get; set; }
        string PhoneNumber { get; set; }
    }
}