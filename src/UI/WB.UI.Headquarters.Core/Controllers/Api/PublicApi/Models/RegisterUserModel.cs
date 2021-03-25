using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class RegisterUserModel
    {
        private string email;

        [Required]
        public Roles Role { get; set; }

        [Required]
        public string UserName { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string Email
        {
            get => email;
            set => email = string.IsNullOrEmpty(value) ? null : value;
        }

        [Required]
        public string Password { get; set; }

        [Description("Login of supervisor for registered interviewer")]
        public string Supervisor { get; set; }

        public Main.Core.Entities.SubEntities.UserRoles GetDomainRole()
        {
            return (Main.Core.Entities.SubEntities.UserRoles) Role;
        }
    }
}
