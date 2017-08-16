using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
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
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserViewFactory usersFactory;
        private readonly HqUserManager userManager;
        private readonly IInterviewerVersionReader interviewerVersionReader;

        public UsersApiController(
            ICommandService commandService,
            IAuthorizedUser authorizedUser,
            ILogger logger,
            IUserViewFactory usersFactory,
            HqUserManager userManager, 
            IInterviewerVersionReader interviewerVersionReader)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.usersFactory = usersFactory;
            this.userManager = userManager;
            this.interviewerVersionReader = interviewerVersionReader;
        }

        [HttpPost]
        [CamelCase]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public DataTableResponse<InterviewerListItem> AllInterviewers([FromBody] DataTableRequestWithFilter reqest)
        {
            Guid? supervisorId = null;

            if (!string.IsNullOrWhiteSpace(reqest.SupervisorName))
                supervisorId = this.userManager.FindByName(reqest.SupervisorName)?.Id;

            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            var currentUserRole = this.authorizedUser.Role;
            if (currentUserRole == UserRoles.Supervisor)
                supervisorId = this.authorizedUser.Id;

            var interviewerApkVersion = interviewerVersionReader.Version;

            var interviewers = this.usersFactory.GetInterviewers(reqest.PageIndex, 
                reqest.PageSize, 
                reqest.GetSortOrder(), 
                reqest.Search.Value, 
                reqest.Archived, 
                reqest.InterviewerOptionFilter, 
                interviewerApkVersion,
                supervisorId,
                reqest.Facet);
            
            return new DataTableResponse<InterviewerListItem>
            {
                Draw = reqest.Draw + 1,
                RecordsTotal = interviewers.TotalCount,
                RecordsFiltered = interviewers.TotalCount,
                Data = interviewers.Items.Select(x => new InterviewerListItem
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    CreationDate =  x.CreationDate.FormatDateWithTime(),
                    SupervisorName = x.SupervisorName,
                    Email = x.Email,
                    DeviceId = x.DeviceId,
                    IsArchived = x.IsArchived,
                    EnumeratorVersion = x.EnumeratorVersion,
                    IsUpToDate = interviewerApkVersion.HasValue && interviewerApkVersion.Value <= x.EnumeratorBuild
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
            public virtual bool IsArchived { get; set; }
            public virtual string EnumeratorVersion { get; set; }
            public bool IsUpToDate { get; set; }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public SupervisorsView Supervisors(UsersListViewModel data)
            => this.usersFactory.GetSupervisors(data.PageIndex, data.PageSize, data.SortOrder.GetOrderRequestString(),
                data.SearchBy, false);

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public SupervisorsView ArchivedSupervisors(UsersListViewModel data)
        {
            return this.usersFactory.GetSupervisors(data.PageIndex, data.PageSize,
                data.SortOrder.GetOrderRequestString(),
                data.SearchBy, true);
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
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public DataTableResponse<SupervisorListItem> AllSupervisors([FromBody] DataTableRequestWithFilter request)
        {
            var users = this.usersFactory.GetSupervisors(request.PageIndex, request.PageSize, request.GetSortOrder(),
                request.Search.Value);

            return new DataTableResponse<SupervisorListItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = users.TotalCount,
                RecordsFiltered = users.TotalCount,
                Data = users.Items.ToList().Select(x => new SupervisorListItem
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    CreationDate = x.CreationDate,
                    Email = x.Email,
                    IsLocked = x.IsLockedByHQ || x.IsLockedBySupervisor,
                    IsArchived = x.IsArchived
                })
            };
        }

        public class SupervisorListItem
        {
            public virtual Guid UserId { get; set; }
            public virtual string UserName { get; set; }
            public virtual string CreationDate { get; set; }
            public virtual string Email { get; set; }
            public virtual bool IsLocked { get; set; }
            public virtual bool IsArchived { get; set; }
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
                    CreationDate = x.CreationDate.FormatDateWithTime(),
                    Email = x.Email,
                    IsLocked = x.IsLockedByHQ || x.IsLockedBySupervisor,
                    IsArchived = x.IsArchived
                })
            };
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<JsonBundleCommandResponse> ArchiveUsers(ArchiveUsersRequest request)
        {
            var archiveResults = request.Archive
                ? await this.userManager.ArchiveUsersAsync(request.UserIds)
                : await this.userManager.UnarchiveUsersAsync(request.UserIds);

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