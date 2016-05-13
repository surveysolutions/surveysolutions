using System.Net.Http;
using System.Web.Http;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Api
{
    public class UsersController : ApiController
    {
        public struct LoginRequestModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public bool StaySignedIn { get; set; }
            public string Recaptcha { get; set; }
        }

        public struct LoginResponseModel
        {
            public LoginStatus Status { get; set; }
        }
        public enum LoginStatus
        {
            Success = 1,
            InvalidLoginOrPassword,
            InvalidCaptcha,
            CaptchaRequired
        }

        private readonly IRecaptchaService recaptchaService;
        private readonly IAuthenticationService authenticationService;
        private readonly IMembershipUserService membership;

        public UsersController(IRecaptchaService recaptchaService, IAuthenticationService authenticationService, IMembershipUserService membership)
        {
            this.recaptchaService = recaptchaService;
            this.authenticationService = authenticationService;
            this.membership = membership;
        }

        [HttpPost]
        [ApiValidationAntiForgeryToken]
        public LoginResponseModel Login(LoginRequestModel model)
        {
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                return new LoginResponseModel() {Status = LoginStatus.InvalidLoginOrPassword};

            var shouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);

            var isCaptchaInvalid = string.IsNullOrEmpty(model.Recaptcha) ||
                                   !this.recaptchaService.IsValid(model.Recaptcha);

            if (shouldShowCaptcha && isCaptchaInvalid)
                return new LoginResponseModel() { Status = LoginStatus.InvalidCaptcha };

            var userIsAuthorized = this.authenticationService.Login(model.UserName, model.Password, model.StaySignedIn);

            if (!userIsAuthorized && !shouldShowCaptcha && this.ShouldShowCaptchaByUserName(model.UserName))
                return new LoginResponseModel() { Status = LoginStatus.CaptchaRequired };

            return new LoginResponseModel() { Status = userIsAuthorized ? LoginStatus.Success : LoginStatus.InvalidLoginOrPassword };
        }


        [Authorize]
        [HttpGet]
        public string CurrentLogin()
        {
            return this.membership.WebUser.UserName;
        }

        private bool ShouldShowCaptchaByUserName(string userName)
        {
            return this.authenticationService.ShouldShowCaptcha() ||
                   this.authenticationService.ShouldShowCaptchaByUserName(userName);
        }
    }
}