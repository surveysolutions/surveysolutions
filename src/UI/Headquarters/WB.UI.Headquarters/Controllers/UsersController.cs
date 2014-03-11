using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Raven.Client;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.Core.Infrastructure.Raven.PlainStorage;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Administrator)]
    public class UsersController : Controller
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
                foreach (ApplicationUser applicationUser in session.Query<ApplicationUser>())
                {
                    viewModel.Users.Add(new UserListDto
                    {
                        Login = applicationUser.UserName,
                        Role = string.Join(", ", applicationUser.Claims.Where(x => x.ClaimType == ClaimTypes.Role).Select(claim => claim.ClaimValue))
                    });
                }
            }
           
            return this.View(viewModel);
        }

        public ActionResult RegisterAccount()
        {
            return this.View();
        }
    }
}