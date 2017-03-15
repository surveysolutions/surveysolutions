using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
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
        private readonly IUserViewFactory usersFactory;

        public UsersApiController(
            ICommandService commandService,
            IIdentityManager identityManager,
            ILogger logger,
            IUserViewFactory usersFactory)
            : base(commandService, logger)
        {
            this.identityManager = identityManager;
            this.usersFactory = usersFactory;
        }


        [HttpPost]
        [CamelCase]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public async Task<DataTableResponse<InterviewerListItem>> AllInterviewers([FromBody] DataTableRequestWithFilter filter)
        {
            Guid? supervisorId = null;

            if (!string.IsNullOrWhiteSpace(filter.SupervisorName))
                supervisorId = (await this.identityManager.GetUserByNameAsync(filter.SupervisorName))?.Id;

            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            var currentUserRole = await this.identityManager.GetRoleForCurrentUserAsync();
            if (currentUserRole == UserRoles.Supervisor)
                supervisorId = this.identityManager.CurrentUserId;

            var interviewers = this.usersFactory.GetInterviewers(filter.PageIndex, filter.PageSize, filter.GetSortOrder(),
                filter.Search.Value, filter.Archived, filter.ConnectedToDevice, supervisorId);

            return new DataTableResponse<InterviewerListItem>
            {
                Draw = filter.Draw + 1,
                RecordsTotal = interviewers.TotalCount,
                RecordsFiltered = interviewers.TotalCount,
                Data = interviewers.Items.ToList().Select(x => new InterviewerListItem
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    CreationDate =  x.CreationDate.ToString(@"MMM dd, YYYY HH:mm"),
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
            => this.usersFactory.GetSupervisors(data.PageIndex, data.PageSize, data.SortOrder.GetOrderRequestString(),
                data.SearchBy, false);

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public SupervisorsView ArchivedSupervisors(UsersListViewModel data)
            => this.usersFactory.GetSupervisors(data.PageIndex, data.PageSize, data.SortOrder.GetOrderRequestString(),
                data.SearchBy, true);

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
            var users = this.usersFactory.GetUsersByRole(request.PageIndex, request.PageSize, request.GetSortOrder(),
                request.Search.Value, false, userRoles);

            return new DataTableResponse<InterviewerListItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = users.TotalCount,
                RecordsFiltered = users.TotalCount,
                Data = users.Items.ToList().Select(x => new InterviewerListItem
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    CreationDate = x.CreationDate.ToString(@"MMM dd, YYYY HH:mm"),
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

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<JsonBundleCommandResponse> ArchiveUsers(ArchiveUsersRequest request)
        {
            var archiveResults = await this.identityManager.ArchiveUsersAsync(request.UserIds, request.Archive);

            return new JsonBundleCommandResponse
            {
                CommandStatuses = archiveResults.Select(x =>
                    new JsonCommandResponse
                    {
                        IsSuccess = x.Succeeded,
                        DomainException = string.Join(@"; ", x.Errors)
                    }).ToList()
            };
        }
    }

    public class ArchiveUsersRequest
    {
        public Guid[] UserIds { get; set; }
        public bool Archive { get; set; }
    }

    public class DeleteSupervisorCommandRequest 
    {
        public Guid SupervisorId { get; set; }
    }
}