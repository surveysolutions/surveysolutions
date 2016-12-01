using System;
using System.Net;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [ApiBasicAuth(new[] { UserRoles.ApiUser, UserRoles.Administrator  }, TreatPasswordAsPlain = true)]
    public class UsersController : BaseApiServiceController
    {
        private readonly IInterviewersViewFactory interviewersFactory;
        private readonly IUserListViewFactory usersFactory;
        private readonly IUserViewFactory userViewFactory;

        public UsersController(ILogger logger,
            IInterviewersViewFactory interviewersFactory,
            IUserListViewFactory usersFactory,
            IUserViewFactory userViewFactory)
            :base(logger)
        {
            this.interviewersFactory = interviewersFactory;
            this.usersFactory = usersFactory;
            this.userViewFactory = userViewFactory;
        }

        [HttpGet]
        [Route("api/v1/supervisors")]
        public UserApiView Supervisors(int limit = 10, int offset = 1)
        {
            var input = new UserListViewInputModel
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
                Role = UserRoles.Supervisor
            };

            var supervisors = this.usersFactory.Load(input);

            return new UserApiView(supervisors);
        }

        [HttpGet]
        [Route("api/v1/supervisors/{supervisorId:guid}/interviewers")]
        public UserApiView Intervievers(Guid supervisorId, int limit = 10, int offset = 1)
        {
            var input = new InterviewersInputModel
            {
                ViewerId = supervisorId,
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
            };

            var interviewers = this.interviewersFactory.Load(input);

            return new UserApiView(interviewers);
        }

        [HttpGet]
        [Route("api/v1/supervisors/{id:guid}/details")]
        [Route("api/v1/interviewers/{id:guid}/details")]
        [Route("api/v1/users/{id:guid}/details")]
        public UserApiDetails Details(Guid id)
        {
            var user = this.userViewFactory.Load(new UserViewInputModel(id));

            if (user == null || user.IsArchived)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return new UserApiDetails(user);
        }
    }
}