using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    public class UsersController : ControllerBase
    {
        private readonly UserManager<DesignerIdentityUser> users;

        public UsersController(UserManager<DesignerIdentityUser> users)
        {
            this.users = users;
        }

        [AuthorizeOrAnonymousQuestionnaire]
        [Route("api/users/CurrentLogin")]
        public async Task<IActionResult> CurrentLogin()
        {
            var user = await this.users.GetUserAsync(User);
            var response = new
            {
                UserName = User.GetUserNameOrNull(),
                Email = user?.Email,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        [Route("api/users/findbyemail")]
        public async Task<IActionResult> Get([FromBody]FindByEmailRequest content)
        {
            if (string.IsNullOrWhiteSpace(content.Query)) return NotFound();

            var account = await this.users.FindByNameOrEmailAsync(content.Query);
            return Ok(new
            {
                doesUserExist = !string.IsNullOrEmpty(account?.UserName),
                userName = account?.UserName,
                email = account?.Email,
                id = account?.Id
            });
        }

        public class FindByEmailRequest
        {
            public string? Query { get; set; }
        }
    }
}
