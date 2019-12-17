using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    public class InterviewersApiController : ApiController
    {
        protected readonly IUserViewFactory userViewFactory;
        protected readonly IAuthorizedUser authorizedUser;
        private readonly HqSignInManager signInManager;

        public InterviewersApiController(
            IUserViewFactory userViewFactory,
            IAuthorizedUser authorizedUser,
            HqSignInManager signInManager)
        {
            this.userViewFactory = userViewFactory;
            this.authorizedUser = authorizedUser;
            this.signInManager = signInManager;
        }

        [ApiBasicAuth(UserRoles.Supervisor)]
        [HttpGet]
        [ApiNoCache]
        public List<InterviewerFullApiView> Get()
        {
            return this.userViewFactory.GetInterviewers(this.authorizedUser.Id).ToList();
        }
    }
}
