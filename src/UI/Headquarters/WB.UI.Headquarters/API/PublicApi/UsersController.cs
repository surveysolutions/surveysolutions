using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.PublicApi
{
    [ApiBasicAuth(new[] { UserRoles.ApiUser, UserRoles.Administrator  }, TreatPasswordAsPlain = true)]
    public class UsersController : BaseApiServiceController
    {
        private readonly IUserViewFactory usersFactory;

        public UsersController(ILogger logger,
            IUserViewFactory usersFactory)
            :base(logger)
        {
            this.usersFactory = usersFactory;
        }

        /// <summary>
        /// Gets list of supervisors
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        [HttpGet]
        [Route("api/v1/supervisors")]
        public UserApiView Supervisors(int limit = 10, int offset = 1)
            => new UserApiView(this.usersFactory.GetUsersByRole(offset, limit, null, null, false, UserRoles.Supervisor));

        /// <summary>
        /// Gets list of interviewers in the specific supervisor team
        /// </summary>
        /// <param name="supervisorId">Id of supervisor</param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        [HttpGet]
        [Route("api/v1/supervisors/{supervisorId:guid}/interviewers")]
        public UserApiView Interviewers(Guid supervisorId, int limit = 10, int offset = 1)
            => new UserApiView(this.usersFactory.GetInterviewers(offset, limit, null, null, false, InterviewerOptionFilter.Any, null, supervisorId));

        /// <summary>
        /// Gets detailed info about single user
        /// </summary>
        /// <param name="id">User id</param>
        [HttpGet]
        [Route("api/v1/supervisors/{id:guid}")]
        [Route("api/v1/users/{id:guid}")]
        public UserApiDetails Details(Guid id)
        {
            var user = this.usersFactory.GetUser(new UserViewInputModel(id));

            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return new UserApiDetails(user);
        }

       
        /// <summary>
        /// Gets detailed info about single interviewer
        /// </summary>
        /// <param name="id">User id</param>
        /// <response code="200"></response>
        /// <response code="404">Interviewer was not found</response>
        [HttpGet]
        [Route("api/v1/interviewers/{id:guid}")]
        public InterviewerUserApiDetails InterviewerDetails(Guid id)
        {
            var user = this.usersFactory.GetUser(new UserViewInputModel(id));

            if (user == null || !user.Roles.Contains(UserRoles.Interviewer))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return new InterviewerUserApiDetails(user);
        }
    }
}