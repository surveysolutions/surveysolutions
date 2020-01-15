using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.API.Filters;

namespace WB.UI.Headquarters.API.Export
{
    [RoutePrefix("api/export/v1")]
    public class UserExportApiController : ApiController
    {
        private readonly IUserRepository userRepository;

        public UserExportApiController(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [Route("user/{id}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            var userId = Guid.Parse(id);
            var userModel = this.userRepository.Users
                .SingleOrDefault(user => user.Id == userId);

            if (userModel == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, $"User with id {id} not found");

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Id = userModel.Id,
                UserName = userModel.UserName,
                Roles = userModel.Roles.Select(r => r.Id.ToUserRole()).ToArray()
            });
        }
    }
}
