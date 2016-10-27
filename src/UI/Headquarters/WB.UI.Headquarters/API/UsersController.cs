using System;
using System.Net;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
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

        [HttpGet]
        [Route("api/v1/supervisors")]
        public UserApiView Supervisors(int limit = 10, int offset = 1)
            => new UserApiView(this.usersFactory.GetUsersByRole(offset, limit, null, null, false, UserRoles.Supervisor));

        [HttpGet]
        [Route("api/v1/supervisors/{supervisorId:guid}/interviewers")]
        public UserApiView Intervievers(Guid supervisorId, int limit = 10, int offset = 1)
            => new UserApiView(this.usersFactory.GetInterviewers(offset, limit, null, null, false, null, supervisorId));

        [HttpGet]
        [Route("api/v1/supervisors/{id:guid}/details")]
        [Route("api/v1/interviewers/{id:guid}/details")]
        [Route("api/v1/users/{id:guid}/details")]
        public UserApiDetails Details(Guid id)
        {
            var user = this.usersFactory.GetUser(new UserViewInputModel(id));

            if (user == null || user.IsArchived)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return new UserApiDetails(user);
        }
    }
}