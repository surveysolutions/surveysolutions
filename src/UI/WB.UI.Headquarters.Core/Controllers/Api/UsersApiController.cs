using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    public class UsersApiController : ControllerBase
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserViewFactory usersFactory;
        private readonly IUserRepository userManager;
        private readonly IInterviewerVersionReader interviewerVersionReader;
        private readonly IExportFactory exportFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IInterviewerProfileFactory interviewerProfileFactory;
        private readonly IUserImportService userImportService;
        private readonly IMoveUserToAnotherTeamService moveUserToAnotherTeamService;
        private readonly IUserArchiveService userArchiveService;
        private readonly IMediator mediator;
        private readonly ILogger<UsersApiController> logger;

        public UsersApiController(
            IAuthorizedUser authorizedUser,
            IUserViewFactory usersFactory,
            IUserRepository userManager,
            IInterviewerVersionReader interviewerVersionReader,
            IExportFactory exportFactory, 
            IInterviewerProfileFactory interviewerProfileFactory,
            IFileSystemAccessor fileSystemAccessor,
            IUserImportService userImportService, 
            IMoveUserToAnotherTeamService moveUserToAnotherTeamService,
            IUserArchiveService userArchiveService,
            IMediator mediator,
            ILogger<UsersApiController> logger)
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
            this.userArchiveService = userArchiveService;
            this.mediator = mediator;
            this.logger = logger;
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public async Task<IActionResult> AllInterviewers(DataTableRequestWithFilter request, [FromQuery] string exportType)
        {
            if (!string.IsNullOrEmpty(exportType))
                return await ExportAllInterviewers(request, exportType);

            Guid? supervisorId = null;

            if (!string.IsNullOrWhiteSpace(request.SupervisorName))
                supervisorId = (await this.userManager.FindByNameAsync(request.SupervisorName))?.Id;

            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            if (authorizedUser.IsSupervisor)
                supervisorId = this.authorizedUser.Id;

            var interviewerApkVersion = await interviewerVersionReader.InterviewerBuildNumber();

            var pageIndex = request.PageIndex;
            var pageSize = request.PageSize;

            var interviewers = this.usersFactory.GetInterviewers(pageIndex,
                pageSize,
                request.GetSortOrder(),
                request.Search?.Value,
                request.Archived,
                interviewerApkVersion,
                supervisorId,
                request.Facet);

            return new JsonResult(new DataTableResponse<InterviewerListItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = interviewers.TotalCount,
                RecordsFiltered = interviewers.TotalCount,
                Data = interviewers.Items.Select(x => new InterviewerListItem
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    FullName = x.FullName,
                    CreationDate = x.CreationDate,
                    SupervisorId = x.SupervisorId,
                    SupervisorName = x.SupervisorName,
                    Email = x.Email,
                    DeviceId = x.DeviceId,
                    IsArchived = x.IsArchived,
                    EnumeratorVersion = x.EnumeratorVersion,
                    IsUpToDate = interviewerApkVersion.HasValue && interviewerApkVersion.Value <= x.EnumeratorBuild,
                    TrafficUsed = (x.TrafficUsed ?? (0L)).InKb(),
                    IsLocked = x.IsLockedByHQ || x.IsLockedBySupervisor,
                    LastLoginDate = x.LastLoginDate
                })
            });
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public async Task<IActionResult> ExportAllInterviewers(DataTableRequestWithFilter request, [FromQuery] string exportType)
        {
            Guid? supervisorId = null;

            Enum.TryParse(exportType, true, out ExportFileType type);

            if (!string.IsNullOrWhiteSpace(request.SupervisorName))
                supervisorId = (await this.userManager.FindByNameAsync(request.SupervisorName))?.Id;

            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            if (this.authorizedUser.IsSupervisor)
                supervisorId = this.authorizedUser.Id;

            var interviewerApkVersion = await interviewerVersionReader.InterviewerBuildNumber();

            var filteredInterviewerIdsToExport = this.usersFactory.GetInterviewersIds(request.Search?.Value,
                request.Archived,
                interviewerApkVersion,
                supervisorId,
                request.Facet);

            var interviewersReport = await this.interviewerProfileFactory.GetInterviewersReport(filteredInterviewerIdsToExport);

            return this.CreateReportResponse(type, interviewersReport, Reports.Interviewers);
        }

        public class UserListItem
        {
            public Guid UserId { get; set; }
            public string UserName { get; set; }
            public DateTime CreationDate { get; set; }
            public string Email { get; set; }
            public bool IsLocked { get; set; }
            public bool IsArchived { get; set; }
        }

        public class InterviewerListItem : UserListItem
        {
            public string FullName { get; set; }
            public string SupervisorName { get; set; }
            public string DeviceId { get; set; }
            public string EnumeratorVersion { get; set; }
            public bool IsUpToDate { get; set; }
            public Guid? SupervisorId { get; set; }
            public long TrafficUsed { get; set; }
            public DateTime? LastLoginDate { get; set; }
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

        [Authorize(Roles = "Administrator, Observer")]
        public DataTableResponse<UserListItem> AllHeadquarters(DataTableRequest request)
        {
            return this.GetUsersInRoleForDataTable(request, UserRoles.Headquarter);
        }

        [Authorize(Roles = "Administrator")]
        public DataTableResponse<UserListItem> AllObservers(DataTableRequest request)
        {
            return this.GetUsersInRoleForDataTable(request, UserRoles.Observer);
        }

        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public DataTableResponse<UserListItem> AllSupervisors(DataTableRequest request) => this.GetUsersInRoleForDataTable(request, UserRoles.Supervisor, null);

        [Authorize(Roles = "Administrator")]
        public DataTableResponse<UserListItem> AllApiUsers(DataTableRequest request)
        {
            return this.GetUsersInRoleForDataTable(request, UserRoles.ApiUser);
        }

        private DataTableResponse<UserListItem> GetUsersInRoleForDataTable(DataTableRequest request,
            UserRoles userRoles, bool? archived = false)
        {
            var users = this.usersFactory.GetUsersByRole(request.PageIndex, request.PageSize, request.GetSortOrder(),
                request.Search?.Value, archived, userRoles);

            return new DataTableResponse<UserListItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = users.TotalCount,
                RecordsFiltered = users.TotalCount,
                Data = users.Items.ToList().Select(x => new UserListItem
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
        public async Task<IActionResult> MoveUserToAnotherTeam([FromBody] MoveUserToAnotherTeamRequest moveRequest)
        {
            var userId = this.authorizedUser.Id;

            var interviewer = userManager.FindById(moveRequest.InterviewerId);
            if (interviewer == null)
                return NotFound();
            
            if (!interviewer.IsInRole(UserRoles.Interviewer) || !interviewer.Profile.SupervisorId.HasValue)
                return NotFound();

            var result = await this.moveUserToAnotherTeamService.Move(
                userId,
                moveRequest.InterviewerId,
                moveRequest.NewSupervisorId, 
                interviewer.Profile.SupervisorId.Value, 
                moveRequest.Mode);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<CommandApiController.JsonCommandResponse> ArchiveUsers([FromBody]ArchiveUsersRequest request)
        {
            try
            {
                if (request.Archive)
                    await this.userArchiveService.ArchiveUsersAsync(request.UserIds);
                else
                    await this.userArchiveService.UnarchiveUsersAsync(request.UserIds);
            }
            catch (UserArchiveException e)
            {
                return new CommandApiController.JsonCommandResponse() { IsSuccess = false, DomainException = e.Message};
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, $"Archive status change error for users ({string.Join(',', request.UserIds)}):", e);
                return new CommandApiController.JsonCommandResponse() { IsSuccess = false, DomainException = "Error occurred" };
            }

            return new CommandApiController.JsonCommandResponse() {IsSuccess = true};
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [Localizable(false)]
        public IActionResult ImportUsersTemplate() => this.CreateReportResponse(ExportFileType.Tab, new ReportView
        {
            Headers = this.userImportService.GetUserProperties(),
            Data = new object[][] { }
        }, Reports.ImportUsersTemplate);

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ApiNoCache]
        [ObservingNotAllowed]
        public async Task<IActionResult> ImportUsers(ImportUsersRequest request)
        {
            if (request?.File == null)
                return this.BadRequest(BatchUpload.Prerequisite_FileOpen);

            var fileExtension = Path.GetExtension(request.File.FileName).ToLower();

            if (!new[] {TextExportFile.Extension, TabExportFile.Extention}.Contains(fileExtension))
                return this.Ok(ToImportError(string.Format(BatchUpload.UploadUsers_NotAllowedExtension, TabExportFile.Extention,
                        TextExportFile.Extension)));

            try
            {
                var openReadStream = request.File.OpenReadStream();

                var importUserErrors = (await this.mediator.Send(new UserImportRequest
                {
                    FileStream = openReadStream,
                    Filename = request.File.FileName
                })).Select(ToImportError).ToArray();

                return this.Ok(importUserErrors);
            }
            catch (PreloadingException e)
            {
                return this.Ok(ToImportError(e.Message));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        public IActionResult CancelToImportUsers()
        {
            this.userImportService.RemoveAllUsersToImport();
            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ApiNoCache]
        public UsersImportStatus ImportStatus() => this.userImportService.GetImportStatus();

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
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

        private ImportUserError[] ToImportError(string message) => new[] {new ImportUserError {Message = message}};

        public class ImportUserError
        {
            public string Line { get; set; }
            public string Column { get; set; }
            public string Message { get; set; }
            public string Description { get; set; }
            public string Recomendation { get; set; }
        }

        private IActionResult CreateReportResponse(ExportFileType type, ReportView report, string reportName)
        {
            var exportFile = this.exportFactory.CreateExportFile(type);

            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(report));
            var fileNameStar = $@"{this.fileSystemAccessor.MakeValidFileName(reportName)}{exportFile.FileExtension}";
            var result = File(exportFileStream, exportFile.MimeType, fileNameStar);
            return result;
        }
    }

    public class ImportUsersRequest
    {
        public IFormFile File { get; set; }
    }

    public class ArchiveUsersRequest
    {
        public Guid[] UserIds { get; set; }
        public bool Archive { get; set; }
    }

    public class MoveUserToAnotherTeamRequest
    {
        public Guid InterviewerId { get; set; }
        public Guid NewSupervisorId { get; set; }
        public MoveUserToAnotherTeamMode Mode { get; set; }
    }

    public class DeleteSupervisorCommandRequest
    {
        public Guid SupervisorId { get; set; }
    }
}
