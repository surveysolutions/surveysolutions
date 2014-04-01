using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Raven.PlainStorage;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Administrator)]
    public class UsersController : BaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRavenPlainStorageProvider storageProvider;

        public UsersController(UserManager<ApplicationUser> userManager,
            IRavenPlainStorageProvider storageProvider)
        {
            this.userManager = userManager;
            this.storageProvider = storageProvider;
        }

        public ActionResult Index()
        {
            var viewModel = new UsersListModel();
            using (var session = storageProvider.GetDocumentStore().OpenSession())
            {
                foreach (ApplicationUser applicationUser in session.Query<ApplicationUser>().OrderBy(x => x.UserName))
                {
                    viewModel.Users.Add(new UserListDto
                    {
                        Id = applicationUser.Id,
                        Login = applicationUser.UserName,
                        Role = string.Join(", ", applicationUser.Claims.Where(x => x.ClaimType == ClaimTypes.Role).Select(claim => claim.ClaimValue))
                    });
                }
            }
            viewModel.HighlightedUser = (string)this.TempData["HighlightedUser"];

            return this.View(viewModel);
        }

        public ActionResult RegisterAccount()
        {
            return this.View(new RegisterAccountModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterAccount(RegisterAccountModel model)
        {
            if (ModelState.IsValid)
            {
                var applicationUser = new ApplicationUser(Guid.NewGuid().FormatGuid())
                {
                    UserName = model.UserName,
                    IsAdministrator = model.AdminRoleEnabled,
                    IsHeadquarter = model.HeadquarterRoleEnabled
                };

                IdentityResult identityResult = await userManager.CreateAsync(applicationUser, model.Password);

                if (identityResult.Succeeded)
                {
                    TempData["HighlightedUser"] = model.UserName;
                    return this.RedirectToAction("Index");
                }

                this.AddErrors(identityResult);
            }

            return View(model);
        }

        public async Task<ActionResult> EditAccount(string id)
        {
            ApplicationUser user = await userManager.FindByIdAsync(id);
            var viewModel = new EditAccountModel
            {
                UserName = user.UserName,
                HeadquarterRoleEnabled = user.IsHeadquarter,
                AdminRoleEnabled = user.IsAdministrator,
                Id = user.Id
            };

            return this.View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAccount(string id, EditAccountModel model)
        {
            ApplicationUser user = await userManager.FindByIdAsync(model.Id);
            user.IsAdministrator = model.AdminRoleEnabled;
            user.IsHeadquarter = model.HeadquarterRoleEnabled;

            if (!string.IsNullOrEmpty(model.Password))
            {
                IdentityResult result = await userManager.PasswordValidator.ValidateAsync(model.Password);

                if (result.Succeeded)
                {
                    string passwordHash = userManager.PasswordHasher.HashPassword(model.Password);
                    user.PasswordHash = passwordHash;
                }
                else
                {
                    this.AddErrors(result);
                    return this.View(model);
                }
            }

            IdentityResult identityResult = await userManager.UpdateAsync(user);
            if (identityResult.Succeeded)
            {
                TempData["HighlightedUser"] = user.UserName;
                return RedirectToAction("Index");
            }

            this.AddErrors(identityResult);
            return this.View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}