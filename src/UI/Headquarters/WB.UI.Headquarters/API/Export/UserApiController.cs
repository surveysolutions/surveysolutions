using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.UI.Headquarters.API.Filters;

namespace WB.UI.Headquarters.API.Export
{
    [RoutePrefix("api/export/v1")]
    public class UserApiController : ApiController
    {
        private readonly IUserRepository userRepository;

        public UserApiController(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [Route("user/{userId}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        public HttpResponseMessage Get(Guid userId)
        {
            var userModel = this.userRepository.Users
                .Where(user => user.Id == userId)
                .Select(user => new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    UserRole = user.Roles.Single()
                });

            return Request.CreateResponse(HttpStatusCode.OK, userModel);
        }
    }
}
