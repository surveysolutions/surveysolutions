using System.Web.Http;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Membership;
using WB.UI.Shared.Web.MembershipProvider.Accounts;

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
            public string ResponseMessage { get; set; }
        }
        public enum LoginStatus
        {
            Success = 1,
            InvalidLoginOrPassword,
            InvalidCaptcha,
            CaptchaRequired,
            IsLockedOut,
            NotApproved
        }

        private readonly IRecaptchaService recaptchaService;
        private readonly IAuthenticationService authenticationService;
        private readonly IMembershipUserService membership;
        private readonly IAccountRepository accountRepository;

        public UsersController(IRecaptchaService recaptchaService, IAuthenticationService authenticationService,
            IMembershipUserService membership, IAccountRepository accountRepository)
        {
            this.recaptchaService = recaptchaService;
            this.authenticationService = authenticationService;
            this.membership = membership;
            this.accountRepository = accountRepository;
        }

        [HttpPost]
        [ApiValidationAntiForgeryToken]
        [AllowAnonymous]
        public LoginResponseModel Login(LoginRequestModel model)
        {
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
            {
                return new LoginResponseModel()
                {
                    Status = LoginStatus.InvalidLoginOrPassword,
                    ResponseMessage = "User name or password is empty"
                };
            }

            var shouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);

            var isCaptchaInvalid = string.IsNullOrEmpty(model.Recaptcha) ||
                                   !this.recaptchaService.IsValid(model.Recaptcha);

            if (shouldShowCaptcha && isCaptchaInvalid)
            {
                return new LoginResponseModel {Status = LoginStatus.InvalidCaptcha};
            }

            var user = this.accountRepository.Get(model.UserName);
            if (user == null)
            {
                return new LoginResponseModel()
                {
                    Status = LoginStatus.InvalidLoginOrPassword,
                    ResponseMessage = "Login or password is incorrect. Please try again"
                };
            }

            if (user.IsLockedOut)
            {
                return new LoginResponseModel()
                {
                    Status = LoginStatus.NotApproved,
                    ResponseMessage = "Your account is blocked. Contact the administrator to unblock your account"
                };
            }

            if (!user.IsConfirmed)
            {
                return new LoginResponseModel()
                {
                    Status = LoginStatus.IsLockedOut,
                    ResponseMessage = "Please, confirm your account first. " +
                                      $"We've sent a confirmation link to {user.Email}. " +
                                      "Didn't get it? " +
                                      $"<a href='{GlobalHelper.GenerateUrl("ResendConfirmation", "Account", new { id = user.UserName })}'>Request another one.</a>"
                };
            }

            var userIsAuthorized = this.authenticationService.Login(model.UserName, model.Password, model.StaySignedIn);

            if (!userIsAuthorized && !shouldShowCaptcha && this.ShouldShowCaptchaByUserName(model.UserName))
            {
                return new LoginResponseModel()
                {
                    Status = LoginStatus.CaptchaRequired
                };
            }

            if (!userIsAuthorized)
            {
                return new LoginResponseModel
                {
                    Status = LoginStatus.InvalidLoginOrPassword,
                    ResponseMessage = "Login or password is incorrect. Please try again"
                };
            }

            return new LoginResponseModel {Status = LoginStatus.Success};
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