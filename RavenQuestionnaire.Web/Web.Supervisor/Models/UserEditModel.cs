using System;
using System.ComponentModel.DataAnnotations;

namespace Web.Supervisor.Models
{
    public class UserEditModel
    {
        [Key]
        public Guid Id { get; set; }
        
        [Display(Name = "User name", Order = 1)]
        [RegularExpression("^[a-zA-Z0-9_]{3,15}$",
            ErrorMessage = "Name needs to be between 3 and 15 characters and contains only letters, digits and underscore symbol")]
        public string UserName { get; set; }

        [RegularExpression("(?=^.{6,255}$)((?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]))^.*", ErrorMessage = "Password must contain at least one number, one upper case character and one lower case character. Length must be between 6 and 255 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Order = 2)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password", Order = 3)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email", Order = 4)]
        public string Email { get; set; }

        [Display(Name = "Is locked", Order = 5)]
        public bool IsLocked { get; set; }
    }
}