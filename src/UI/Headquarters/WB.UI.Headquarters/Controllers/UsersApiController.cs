using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.IntreviewerProfiles;
using WB.Core.BoundedContexts.Headquarters.MoveUserToAnotherTeam;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    public class UsersApiController : BaseApiController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserViewFactory usersFactory;
        private readonly HqUserManager userManager;
        private readonly IInterviewerVersionReader interviewerVersionReader;
        private readonly IExportFactory exportFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IInterviewerProfileFactory interviewerProfileFactory;
        private readonly IUserImportService userImportService;
        private readonly IMoveUserToAnotherTeamService moveUserToAnotherTeamService;

        public UsersApiController(
            ICommandService commandService,
            IAuthorizedUser authorizedUser,
            ILogger logger,
            IUserViewFactory usersFactory,
            HqUserManager userManager,
            IInterviewerVersionReader interviewerVersionReader,
            IExportFactory exportFactory, 
            IInterviewerProfileFactory interviewerProfileFactory,
            IFileSystemAccessor fileSystemAccessor,
            IUserImportService userImportService, 
            IMoveUserToAnotherTeamService moveUserToAnotherTeamService)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.usersFactory = usersFactory;
            this.userManager = userManager;
            this.interviewerVersionReader = interviewerVersionReader;
            this.exportFactory = exportFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewerProfileFactory = interviewerProfileFactory;
            this.userImportService = userImportService;
            this.moveUserToAnotherTeamService = moveUserToAnotherTeamService;
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

            var pageIndex = reqest.PageIndex;
            var pageSize = reqest.PageSize;

            var interviewers = this.usersFactory.GetInterviewers(pageIndex,
                pageSize,
                reqest.GetSortOrder(),
                reqest.Search.Value,
                reqest.Archived,
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
                    CreationDate = x.CreationDate,
                    SupervisorId = x.SupervisorId,
                    SupervisorName = x.SupervisorName,
                    Email = x.Email,
                    DeviceId = x.DeviceId,
                    IsArchived = x.IsArchived,
                    EnumeratorVersion = x.EnumeratorVersion,
                    IsUpToDate = interviewerApkVersion.HasValue && interviewerApkVersion.Value <= x.EnumeratorBuild
                })
            };
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public HttpResponseMessage AllInterviewers([FromUri] DataTableRequestWithFilter reqest, [FromUri] string exportType)
        {
            Guid? supervisorId = null;

            Enum.TryParse(exportType, true, out ExportFileType type);

            if (!string.IsNullOrWhiteSpace(reqest.SupervisorName))
                supervisorId = this.userManager.FindByName(reqest.SupervisorName)?.Id;

            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            var currentUserRole = this.authorizedUser.Role;
            if (currentUserRole == UserRoles.Supervisor)
                supervisorId = this.authorizedUser.Id;

            var interviewerApkVersion = interviewerVersionReader.Version;

            var filteredInterviewerIdsToExport = this.usersFactory.GetInterviewersIds(reqest.Search.Value,
                reqest.Archived,
                interviewerApkVersion,
                supervisorId,
                reqest.Facet);

            var interviewersReport = this.interviewerProfileFactory.GetInterviewersReport(filteredInterviewerIdsToExport);

            return this.CreateReportResponse(type, interviewersReport, Reports.Interviewers);
        }

        public class InterviewerListItem
        {
            public virtual Guid UserId { get; set; }
            public virtual string UserName { get; set; }
            public virtual DateTime CreationDate { get; set; }
            public virtual string SupervisorName { get; set; }
            public virtual string Email { get; set; }
            public virtual string DeviceId { get; set; }
            public virtual bool IsLocked { get; set; }
            public virtual bool IsArchived { get; set; }
            public virtual string EnumeratorVersion { get; set; }
            public bool IsUpToDate { get; set; }
            public virtual Guid? SupervisorId { get; set; }
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
            public virtual DateTime CreationDate { get; set; }
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

        private DataTableResponse<InterviewerListItem> GetUsersInRoleForDataTable(DataTableRequest request,
            UserRoles userRoles)
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
                    CreationDate = x.CreationDate,
                    Email = x.Email,
                    IsLocked = x.IsLockedByHQ || x.IsLockedBySupervisor,
                    IsArchived = x.IsArchived
                })
            };
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<MoveInterviewerToAnotherTeamResult> MoveUserToAnotherTeam(MoveUserToAnotherTeamRequest moveRequest)
        {
            var userId = this.authorizedUser.Id;
            var result = await this.moveUserToAnotherTeamService.Move(
                userId,
                moveRequest.InterviewerId,
                moveRequest.NewSupervisorId, 
                moveRequest.OldSupervisorId, 
                moveRequest.Mode);

            return result;
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

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [Localizable(false)]
        public HttpResponseMessage ImportUsersTemplate() => this.CreateReportResponse(ExportFileType.Tab, new ReportView
        {
            Headers = this.userImportService.GetUserProperties(),
            Data = new object[][] { }
        }, Reports.ImportUsersTemplate);

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        [CamelCase]
        [ApiNoCache]
        [ObserverNotAllowedApi]
        public IHttpActionResult ImportUsers(ImportUsersRequest request)
        {
            if (request?.File?.FileBytes == null)
                return this.BadRequest(BatchUpload.Prerequisite_FileOpen);

            var fileExtension = Path.GetExtension(request.File.FileName).ToLower();

            if (!new[] {TextExportFile.Extension, TabExportFile.Extention}.Contains(fileExtension))
                return this.BadRequest(string.Format(BatchUpload.UploadUsers_NotAllowedExtension, TabExportFile.Extention, TextExportFile.Extension));

            try
            {
                var importUserErrors = this.userImportService.VerifyAndSaveIfNoErrors(request.File.FileBytes, request.File.FileName)
                    .Take(8).Select(ToImportError).ToArray();
                return this.Ok(importUserErrors);
            }
            catch (PreloadingException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        [CamelCase]
        public void CancelToImportUsers() => this.userImportService.RemoveAllUsersToImport();

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [CamelCase]
        [ApiNoCache]
        public UsersImportStatus ImportStatus() => this.userImportService.GetImportStatus();

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [CamelCase]
        [ApiNoCache]
        public UsersImportCompleteStatus ImportCompleteStatus() => this.userImportService.GetImportCompleteStatus();

        private ImportUserError ToImportError(UserImportVerificationError error) => new ImportUserError
        {
            Line = error.RowNumber.ToString(@"D2"),
            Column = error.ColumnName.ToLower(),
            Message = ToImportErrorMessage(error.Code),
            Description = ToImportErrorDescription(error.Code, error.CellValue),
            Recomendation = ToImportErrorRecomendation(error.Code)
        };

        private string ToImportErrorRecomendation(string errorCode)
            => UserPreloadingVerificationMessages.ResourceManager.GetString($"{errorCode}Recomendation");

        private string ToImportErrorDescription(string errorCode, string errorCellValue)
            => string.Format(UserPreloadingVerificationMessages.ResourceManager.GetString($"{errorCode}Description"), errorCellValue);

        private string ToImportErrorMessage(string errorCode)
            => UserPreloadingVerificationMessages.ResourceManager.GetString(errorCode);


        public class ImportUserError
        {
            public string Line { get; set; }
            public string Column { get; set; }
            public string Message { get; set; }
            public string Description { get; set; }
            public string Recomendation { get; set; }
        }

        private HttpResponseMessage CreateReportResponse(ExportFileType type, ReportView report, string reportName)
        {
            var exportFile = this.exportFactory.CreateExportFile(type);

            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(report));
            var result = new ProgressiveDownload(this.Request).ResultMessage(exportFileStream, exportFile.MimeType);

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileNameStar = $@"{this.fileSystemAccessor.MakeValidFileName(reportName)}{exportFile.FileExtension}"
            };

            return result;
        }
    }

    public class ImportUsersRequest
    {
        public HttpFile File { get; set; }
    }

    public class ArchiveUsersRequest
    {
        public Guid[] UserIds { get; set; }
        public bool Archive { get; set; }
    }

    public class MoveUserToAnotherTeamRequest
    {
        public Guid InterviewerId { get; set; }
        public Guid OldSupervisorId { get; set; }
        public Guid NewSupervisorId { get; set; }
        public MoveUserToAnotherTeamMode Mode { get; set; }
    }

    public class DeleteSupervisorCommandRequest
    {
        public Guid SupervisorId { get; set; }
    }
}
