using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class RegisterUserModel
    {
        [Required]
        public UserRoles Role { get; set; }

        [Required]
        public string UserName { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Description("Login of supervisor for registered interviewer")]
        public string Supervisor { get; set; }
    }
}
