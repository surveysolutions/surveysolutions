namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.Web.Mvc;

    [DisplayName("Password reset")]
    public class ResetPasswordConfirmationModel : PasswordModel
    {
        [HiddenInput(DisplayValue = false)]
        public string Token { get; set; }
    }
}