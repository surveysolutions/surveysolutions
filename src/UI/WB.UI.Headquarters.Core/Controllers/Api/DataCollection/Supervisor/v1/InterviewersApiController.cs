using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    public class InterviewersControllerBase : ControllerBase
    {
        protected readonly IUserViewFactory userViewFactory;
        protected readonly IAuthorizedUser authorizedUser;

        public InterviewersControllerBase(
            IUserViewFactory userViewFactory,
            IAuthorizedUser authorizedUser)
        {
            this.userViewFactory = userViewFactory;
            this.authorizedUser = authorizedUser;
        }

        [Authorize(Roles = "Supervisor")]
        [HttpGet]
        [Route("api/supervisor/v1/interviewers")]
        public ActionResult<List<InterviewerFullApiView>> Get()
        {
            return this.userViewFactory.GetInterviewers(this.authorizedUser.Id).ToList();
        }
    }
}
