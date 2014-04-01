using System;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.BoundedContexts.Headquarters.Team.Models;
using WB.UI.Headquarters.Api.Models;

namespace WB.UI.Headquarters.Api
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class UsersApiController : ApiController
    {
        private readonly IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory;
        private readonly IViewFactory<SupervisorInterviewersInputModel, InterviewersView> supervisorInterviewersFactory;

        public UsersApiController(
            IViewFactory<UserListViewInputModel, UserListView> supervisorsFactory,
            IViewFactory<SupervisorInterviewersInputModel, InterviewersView> supervisorInterviewersFactory)
        {
            this.supervisorsFactory = supervisorsFactory;
            this.supervisorInterviewersFactory = supervisorInterviewersFactory;
        }

        public UserListView Supervisors(UsersListViewModel data)
        {
            var input = new UserListViewInputModel
                {
                    Role = UserRoles.Supervisor,
                    Orders = data.SortOrder
                };

            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            return this.supervisorsFactory.Load(input);
        }

        public InterviewersView Interviewers(UsersListViewModel data)
        {
            var input = new SupervisorInterviewersInputModel(data.Request.SupervisorId.Value)
            {
                Orders = data.SortOrder
            };

            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            return this.supervisorInterviewersFactory.Load(input);
        }
    }
}