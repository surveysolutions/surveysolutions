using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
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

        public InterviewersApiController(
            IUserViewFactory userViewFactory,
            IAuthorizedUser authorizedUser)
        {
            this.userViewFactory = userViewFactory;
            this.authorizedUser = authorizedUser;
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
