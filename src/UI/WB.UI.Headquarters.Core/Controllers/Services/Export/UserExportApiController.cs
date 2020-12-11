using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.UI.Headquarters.Controllers.Services.Export
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public UserRoles[] Roles { get; set; }
    }

    [Route("api/export/v1")]
    [Authorize(AuthenticationSchemes = AuthType.TenantToken)]
    public class UserExportApiController : Controller
    {
        private readonly IUserRepository userRepository;

        public UserExportApiController(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [Route("user/{id}")]
        [HttpGet]
        public ActionResult<UserDto> Get(string id)
        {
            var userId = Guid.Parse(id);
            var userModel = this.userRepository.Users
                .SingleOrDefault(user => user.Id == userId);

            if (userModel == null) return NotFound($"User with id {id} not found");

            return new UserDto {Id = userModel.Id, UserName = userModel.UserName, Roles = userModel.Roles.Select(r => r.Id.ToUserRole()).ToArray()};
        }
    }
}
