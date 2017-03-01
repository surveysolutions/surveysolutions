using System;
using System.Linq;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    public class UsersApiController : BaseApiController
    {
        private readonly IIdentityManager identityManager;
        private readonly IInterviewersViewFactory interviewersFactory;
        private readonly ISupervisorsViewFactory supervisorsFactory;
        private readonly IUserListViewFactory usersFactory;

        public UsersApiController(
            ICommandService commandService,
            IIdentityManager identityManager,
            ILogger logger,
            IInterviewersViewFactory interviewersFactory,
            ISupervisorsViewFactory supervisorsFactory,
            IUserListViewFactory usersFactory)
            : base(commandService, logger)
        {
            this.identityManager = identityManager;
            this.interviewersFactory = interviewersFactory;
            this.supervisorsFactory = supervisorsFactory;
            this.usersFactory = usersFactory;
        }


        [HttpPost]
        [CamelCase]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public DataTableResponse<InterviewerListItem> AllInterviewers([FromBody] DataTableRequestWithFilter request)
        {
            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            Guid viewerId = this.identityManager.CurrentUserId;

            var input = new InterviewersInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                ViewerId = viewerId,
                Orders = request.GetSortOrderRequestItems(),
                SearchBy = request.Search.Value,

                SupervisorName = request.SupervisorName,
                Archived = request.Archived,
                ConnectedToDevice = request.ConnectedToDevice
            };

            var interviewers = this.interviewersFactory.Load(input);

            return new DataTableResponse<InterviewerListItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = interviewers.TotalCount,
                RecordsFiltered = interviewers.TotalCount,
                Data = interviewers.Items.ToList().Select(x => new InterviewerListItem
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    CreationDate =  x.CreationDate,

                    SupervisorName = x.SupervisorName,
                    Email = x.Email,
                    DeviceId = x.DeviceId
                })
            };

        }

        public class InterviewerListItem
        {
            public virtual Guid UserId { get; set; }
            public virtual string UserName { get; set; }
            public virtual string CreationDate { get; set; }
            public virtual string SupervisorName { get; set; }
            public virtual string Email { get; set; }
            public virtual string DeviceId { get; set; }
            public virtual bool IsLocked { get; set; }
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
        [CamelCase]
        [Authorize(Roles = "Administrator, Observer")]
        public DataTableResponse<InterviewerListItem> AllHeadquarters([FromBody] DataTableRequest request)
        {
            return this.GetUsersInRoleForDataTable(request, UserRoles.Headquarter);
        }

        [HttpPost]
        [CamelCase]
        [Authorize(Roles = "Administrator")]
        public DataTableResponse<InterviewerListItem> AllObservers([FromBody] DataTableRequest request)
        {
            return this.GetUsersInRoleForDataTable(request, UserRoles.Observer);
        }


        [HttpPost]
        [CamelCase]
        [Authorize(Roles = "Administrator")]
        public DataTableResponse<InterviewerListItem> AllSupervisors([FromBody] DataTableRequest request)
        {
            return this.GetUsersInRoleForDataTable(request, UserRoles.Supervisor);
        }

        [HttpPost]
        [CamelCase]
        [Authorize(Roles = "Administrator")]
        public DataTableResponse<InterviewerListItem> AllApiUsers([FromBody] DataTableRequest request)
        {
            return this.GetUsersInRoleForDataTable(request, UserRoles.ApiUser);
        }

        private DataTableResponse<InterviewerListItem> GetUsersInRoleForDataTable(DataTableRequest request, UserRoles userRoles)
        {
            var input = new UserListViewInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Role = userRoles,
                Orders = request.GetSortOrderRequestItems(),
                SearchBy = request.Search.Value,
            };

            var headquarters = this.usersFactory.Load(input);

            return new DataTableResponse<InterviewerListItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = headquarters.TotalCount,
                RecordsFiltered = headquarters.TotalCount,
                Data = headquarters.Items.ToList().Select(x => new InterviewerListItem
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    CreationDate = x.CreationDate,
                    Email = x.Email,
                    IsLocked = x.IsLockedByHQ || x.IsLockedBySupervisor
                })
            };
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public JsonCommandResponse DeleteSupervisor(DeleteSupervisorCommandRequest request)
        {
            var identityResults = this.identityManager.DeleteSupervisorAndDependentInterviewers(request.SupervisorId).ToList();

            return new JsonCommandResponse
            {
                IsSuccess = identityResults.All(result => result.Succeeded),
                DomainException = string.Join(@"; ", identityResults.Select(result=>result.Errors))
            };
        }
    }
    public class DeleteSupervisorCommandRequest 
    {
        public Guid SupervisorId { get; set; }
    }
}