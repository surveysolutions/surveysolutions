using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    public class UsersController : ControllerBase
    {
        private readonly UserManager<DesignerIdentityUser> users;
        private readonly IPlainKeyValueStorage<AssistantSettings> appSettingsStorage;

        public UsersController(UserManager<DesignerIdentityUser> users,
            IPlainKeyValueStorage<AssistantSettings> appSettingsStorage)
        {
            this.users = users;
            this.appSettingsStorage = appSettingsStorage;
        }

        [Route("api/users/CurrentLogin")]
        public async Task<IActionResult> CurrentLogin()
        {
            var user = await this.users.GetUserAsync(User);
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            
            var settings = appSettingsStorage.GetById(AssistantSettings.AssistantSettingsKey);
            
            var response = new
            {
                UserName = User.GetUserNameOrNull(),
                Email = user?.Email,
                IsAuthenticated = isAuthenticated,
                AIAvailable = isAuthenticated && settings != null && settings.IsEnabled && settings.IsAvailableToAllUsers
                //check if AI assistant is enabled for current user
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
