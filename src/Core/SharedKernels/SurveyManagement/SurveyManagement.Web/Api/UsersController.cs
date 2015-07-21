using System;
using System.Net;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class UsersController : BaseApiServiceController
    {
        private readonly IInterviewersViewFactory interviewersFactory;
        private readonly IUserListViewFactory supervisorsFactory;
        private readonly IUserViewFactory userViewFactory;

        public UsersController(ILogger logger,
            IInterviewersViewFactory interviewersFactory,
            IUserListViewFactory supervisorsFactory,
            IUserViewFactory userViewFactory)
            :base(logger)
        {
            this.interviewersFactory = interviewersFactory;
            this.supervisorsFactory = supervisorsFactory;
            this.userViewFactory = userViewFactory;
        }

        [HttpGet]
        [Route("apis/v1/supervisors")]
        public UserApiView Supervisors(int limit = 10, int offset = 1)
        {
            var input = new UserListViewInputModel
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
                Role = UserRoles.Supervisor
            };

            var supervisors = this.supervisorsFactory.Load(input);

            return new UserApiView(supervisors);
        }

        [HttpGet]
        [Route("apis/v1/supervisors/{supervisorId:guid}/interviewers")]
        public UserApiView Intervievers(Guid supervisorId, int limit = 10, int offset = 1)
        {
            var input = new InterviewersInputModel(supervisorId)
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
            };

            var interviewers = this.interviewersFactory.Load(input);

            return new UserApiView(interviewers);
        }

        [HttpGet]
        [Route("apis/v1/supervisors/{id:guid}/details")]
        [Route("apis/v1/interviewers/{id:guid}/details")]
        [Route("apis/v1/users/{id:guid}/details")]
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