using System;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    public class UsersApiController : BaseApiController
    {
        private readonly IInterviewersViewFactory interviewersFactory;
        private readonly IUserListViewFactory supervisorsFactory;

        public UsersApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IInterviewersViewFactory interviewersFactory,
            IUserListViewFactory supervisorsFactory)
            : base(commandService, provider, logger)
        {
            this.interviewersFactory = interviewersFactory;
            this.supervisorsFactory = supervisorsFactory;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public InterviewersView Interviewers(UsersListViewModel data)
        {
            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            Guid? viewerId = this.GlobalInfo.IsHeadquarter || this.GlobalInfo.IsAdministrator
                                 ? data.Request.SupervisorId
                                 : this.GlobalInfo.GetCurrentUser().Id;
            if (viewerId != null)
            {
                var input = new InterviewersInputModel(viewerId.Value)
                    {
                        Orders = data.SortOrder,
                        SearchBy = data.SearchBy
                    };
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                return this.interviewersFactory.Load(input);
            }

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public InterviewersView ArchivedInterviewers(UsersListViewModel data)
        {
            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            Guid? viewerId = this.GlobalInfo.IsHeadquarter || this.GlobalInfo.IsAdministrator
                                 ? data.Request.SupervisorId
                                 : this.GlobalInfo.GetCurrentUser().Id;
            if (viewerId != null)
            {
                var input = new InterviewersInputModel(viewerId.Value)
                {
                    Orders = data.SortOrder,
                    SearchBy = data.SearchBy,
                    Archived = true
                };
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                return this.interviewersFactory.Load(input);
            }

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public UserListView Supervisors(UsersListViewModel data)
        {
            return GetUsers(data, UserRoles.Supervisor);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public UserListView ArchivedSupervisors(UsersListViewModel data)
        {
            return GetUsers(data, UserRoles.Supervisor, archived: true);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Observer")]
        public UserListView Headquarters(UsersListViewModel data)
        {
            return GetUsers(data, UserRoles.Headquarter);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public UserListView Observers(UsersListViewModel data)
        {
            return GetUsers(data, UserRoles.Observer);
        }

        private UserListView GetUsers(UsersListViewModel data, UserRoles role, bool archived = false)
        {
            var input = new UserListViewInputModel
            {
                Role = role,
                Orders = data.SortOrder,
                SearchBy = data.SearchBy,
                Archived = archived
            };

            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            return this.supervisorsFactory.Load(input);
        }
    }
}