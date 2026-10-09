using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Areas.Identity.Pages.Account;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit.Designer.Areas.Identity.Pages.Account
{
    [TestFixture]
    [TestOf(typeof(RegisterModel))]
    public class RegisterModelTests
    {
        [Test]
        public async Task when_recaptcha_v3_score_is_at_threshold_with_matching_action_should_attempt_user_creation()
        {
            var userManager = CreateUserManager();
            userManager.Setup(x => x.CreateAsync(It.IsAny<DesignerIdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "failure" }));

            var model = CreateRegisterModel(
                userManager.Object,
                new RecaptchaResponse { success = true, score = 0.5, action = "register" });

            var result = await model.OnPostAsync("/return");

            Assert.That(result, Is.InstanceOf<PageResult>());
            userManager.Verify(x => x.CreateAsync(It.IsAny<DesignerIdentityUser>(), It.IsAny<string>()), Times.Once);
        }

        [TestCase(true, 0.49, "register")]
        [TestCase(false, 0.9, "register")]
        [TestCase(true, 0.9, "other")]
        public async Task when_recaptcha_v3_response_is_rejected_should_not_create_user(bool success, double score, string action)
        {
            var userManager = CreateUserManager();
            var model = CreateRegisterModel(
                userManager.Object,
                new RecaptchaResponse { success = success, score = score, action = action });

            var result = await model.OnPostAsync("/return");

            Assert.That(result, Is.InstanceOf<PageResult>());
            Assert.That(model.ErrorMessage, Is.EqualTo(ErrorMessages.You_did_not_type_the_verification_word_correctly));
            userManager.Verify(x => x.CreateAsync(It.IsAny<DesignerIdentityUser>(), It.IsAny<string>()), Times.Never);
        }

        private static RegisterModel CreateRegisterModel(UserManager<DesignerIdentityUser> userManager, RecaptchaResponse recaptchaResponse)
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.IsLocalUrl(It.IsAny<string>()))
                .Returns(true);

            var recaptchaService = new Mock<IRecaptchaService>();
            recaptchaService.Setup(x => x.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()))
                .ReturnsAsync(recaptchaResponse);

            return new RegisterModel(
                userManager,
                Mock.Of<IViewRenderService>(),
                Mock.Of<ILogger<RegisterModel>>(),
                Mock.Of<IEmailSender>(),
                recaptchaService.Object,
                Options.Create(new CaptchaConfig
                {
                    CaptchaType = CaptchaProviderType.RecaptchaV3,
                    RecaptchaV3MinimumScore = 0.5
                }))
            {
                Url = urlHelper.Object,
                PageContext = new PageContext
                {
                    HttpContext = new DefaultHttpContext()
                },
                Input = new RegisterModel.InputModel
                {
                    Login = "tester",
                    Email = "tester@example.com",
                    Password = "pwd",
                    ConfirmPassword = "pwd"
                }
            };
        }

        private static Mock<UserManager<DesignerIdentityUser>> CreateUserManager()
        {
            var store = new Mock<IUserStore<DesignerIdentityUser>>();
            return new Mock<UserManager<DesignerIdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
