using System;
using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.DataAnnotations;

namespace WB.UI.Headquarters.Models.Users
{

    public class TwoFAUser
    {
        public Guid UserId { get; set; }
    }

    public class VerificationCodeModel: TwoFAUser
    {
        public string VerificationCode { get; set; }
    }
}
