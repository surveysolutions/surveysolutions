using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer1.Extensions;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    public class UsersController : ControllerBase
    {
        private readonly UserManager<DesignerIdentityUser> users;

        public UsersController(UserManager<DesignerIdentityUser> users)
        {
            this.users = users;
        }

        [Authorize]
        [Route("api/users/CurrentLogin")]
        public async Task<IActionResult> CurrentLogin()
        {
            var user = await this.users.GetUserAsync(User);
            var response = new
            {
                UserName = User.GetUserName(),
                Email = user.Email
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        [Route("api/users/findbyemail")]
        public async Task<IActionResult> Get([FromQuery]string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return NotFound();

            var account = await this.users.FindByNameAsync(q) ?? 
                          await this.users.FindByEmailAsync(q);
            return Ok(new
            {
                doesUserExist = !string.IsNullOrEmpty(account?.UserName),
                userName = account?.UserName,
                email = account?.Email,
                id = account?.Id
            });
        }
    }
}
