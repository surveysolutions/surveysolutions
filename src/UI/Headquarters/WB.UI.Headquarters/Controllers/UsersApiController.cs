using System;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteSupervisor;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    public class UsersApiController : BaseApiController
    {
        private readonly IInterviewersViewFactory interviewersFactory;
        private readonly ISupervisorsViewFactory supervisorsFactory;
        private readonly IUserListViewFactory usersFactory;
        public readonly IDeleteSupervisorService deleteSupervisorService;

        public UsersApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IInterviewersViewFactory interviewersFactory,
            ISupervisorsViewFactory supervisorsFactory,
            IUserListViewFactory usersFactory, 
            IDeleteSupervisorService deleteSupervisorService)
            : base(commandService, provider, logger)
        {
            this.interviewersFactory = interviewersFactory;
            this.supervisorsFactory = supervisorsFactory;
            this.usersFactory = usersFactory;
            this.deleteSupervisorService = deleteSupervisorService;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public InterviewersView Interviewers(InterviewersListViewModel filter)
        {
            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            Guid viewerId = this.GlobalInfo.GetCurrentUser().Id;

            var input = new InterviewersInputModel
            {
                Page = filter.PageIndex,
                PageSize = filter.PageSize,
                ViewerId = viewerId,
                SupervisorName = filter.SupervisorName,
                Orders = filter.SortOrder,
                SearchBy = filter.SearchBy,
                Archived = filter.Archived,
                ConnectedToDevice = filter.ConnectedToDevice
            };

            return this.interviewersFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public SupervisorsView Supervisors(UsersListViewModel data)
        {
            var input = new SupervisorsInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Orders = data.SortOrder,
                SearchBy = data.SearchBy,
                Archived = false
            };

            return this.supervisorsFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public SupervisorsView ArchivedSupervisors(UsersListViewModel data)
        {
            var input = new SupervisorsInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Orders = data.SortOrder,
                SearchBy = data.SearchBy,
                Archived = true
            };

            return this.supervisorsFactory.Load(input);
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

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public UserListView ApiUsers(UsersListViewModel data)
        {
            return GetUsers(data, UserRoles.ApiUser);
        }

        private UserListView GetUsers(UsersListViewModel data, UserRoles role, bool archived = false)
        {
            var input = new UserListViewInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Role = role,
                Orders = data.SortOrder,
                SearchBy = data.SearchBy,
                Archived = archived
            };

            return this.usersFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public JsonCommandResponse DeleteSupervisor(DeleteSupervisorCommandRequest request)
        {
            var response = new JsonCommandResponse();
            try
            {
                deleteSupervisorService.DeleteSupervisor(request.SupervisorId);
                response.IsSuccess = true;
            }
            catch (Exception e)
            {
                this.Logger.Error(e.Message, e);

                response.IsSuccess = false;
                response.DomainException = e.Message;
            }

            return response;
        }
    }
    public class DeleteSupervisorCommandRequest 
    {
        public Guid SupervisorId { get; set; }
    }
}