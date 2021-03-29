using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class ChangePasswordRequest
    {
        [Required]
        public string NewPassword { get; set; }
    }
}