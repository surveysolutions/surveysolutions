﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using WB.UI.Headquarters.Controllers.Api;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Resources;


namespace WB.UI.Headquarters.Controllers
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
            IUserArchiveService userArchiveService)
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
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public async Task<DataTableResponse<InterviewerListItem>> AllInterviewers([FromBody] DataTableRequestWithFilter request)
        {
            Guid? supervisorId = null;

            if (!string.IsNullOrWhiteSpace(request.SupervisorName))
                supervisorId = (await this.userManager.FindByNameAsync(request.SupervisorName))?.Id;

            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            if (authorizedUser.IsSupervisor)
                supervisorId = this.authorizedUser.Id;

            var interviewerApkVersion = interviewerVersionReader.InterviewerBuildNumber;

            var pageIndex = request.PageIndex;
            var pageSize = request.PageSize;

            var interviewers = this.usersFactory.GetInterviewers(pageIndex,
                pageSize,
                request.GetSortOrder(),
                request.Search.Value,
                request.Archived,
                interviewerApkVersion,
                supervisorId,
                request.Facet);

            return new DataTableResponse<InterviewerListItem>
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
                })
            };
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public async Task<IActionResult> AllInterviewers(DataTableRequestWithFilter reqest, [FromQuery] string exportType)
        {
            Guid? supervisorId = null;

            Enum.TryParse(exportType, true, out ExportFileType type);

            if (!string.IsNullOrWhiteSpace(reqest.SupervisorName))
                supervisorId = (await this.userManager.FindByNameAsync(reqest.SupervisorName))?.Id;

            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            if (this.authorizedUser.IsSupervisor)
                supervisorId = this.authorizedUser.Id;

            var interviewerApkVersion = interviewerVersionReader.InterviewerBuildNumber;

            var filteredInterviewerIdsToExport = this.usersFactory.GetInterviewersIds(reqest.Search.Value,
                reqest.Archived,
                interviewerApkVersion,
                supervisorId,
                reqest.Facet);

            var interviewersReport = this.interviewerProfileFactory.GetInterviewersReport(filteredInterviewerIdsToExport);

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
        public DataTableResponse<UserListItem> AllSupervisors(DataTableRequest request) => this.GetUsersInRoleForDataTable(request, UserRoles.Supervisor);

        [Authorize(Roles = "Administrator")]
        public DataTableResponse<UserListItem> AllApiUsers(DataTableRequest request)
        {
            return this.GetUsersInRoleForDataTable(request, UserRoles.ApiUser);
        }

        private DataTableResponse<UserListItem> GetUsersInRoleForDataTable(DataTableRequest request,
            UserRoles userRoles)
        {
            var users = this.usersFactory.GetUsersByRole(request.PageIndex, request.PageSize, request.GetSortOrder(),
                request.Search?.Value, false, userRoles);

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
        public async Task<CommandApiController.JsonBundleCommandResponse> ArchiveUsers(ArchiveUsersRequest request)
        {
            if (request.Archive)
                await this.userArchiveService.ArchiveUsersAsync(request.UserIds);
            else
                await this.userArchiveService.UnarchiveUsersAsync(request.UserIds);

            throw new ArgumentException("Need implement archive and unarchive");

            //return new CommandApiController.JsonBundleCommandResponse
            //{
            //    CommandStatuses = new List<CommandApiController.JsonCommandResponse>
            //    {
            //        new CommandApiController.JsonCommandResponse
            //        {
            //            IsSuccess = true
            //        }
            //    }
            //};


            /*return new CommandApiController.JsonBundleCommandResponse
            {
                CommandStatuses = archiveResults.Select(x =>
                    new CommandApiController.JsonCommandResponse
                    {
                        IsSuccess = x.Succeeded,
                        DomainException = string.Join(@"; ", x.Errors)
                    }).ToList()
            };*/
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
        [ObserverNotAllowed]
        public async Task<IActionResult> ImportUsers(ImportUsersRequest request)
        {
            if (request?.File == null)
                return this.BadRequest(BatchUpload.Prerequisite_FileOpen);

            var fileExtension = Path.GetExtension(request.File.FileName).ToLower();

            if (!new[] {TextExportFile.Extension, TabExportFile.Extention}.Contains(fileExtension))
                return this.BadRequest(string.Format(BatchUpload.UploadUsers_NotAllowedExtension, TabExportFile.Extention, TextExportFile.Extension));

            try
            {
                var importUserErrors = this.userImportService.VerifyAndSaveIfNoErrors(request.File.OpenReadStream(), request.File.FileName)
                    .Take(8).Select(ToImportError).ToArray();

                if (!importUserErrors.Any())
                    await this.userImportService.ScheduleRunUserImportAsync();

                return this.Ok(importUserErrors);
            }
            catch (PreloadingException e)
            {
                return this.BadRequest(e.Message);
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
        public Guid OldSupervisorId { get; set; }
        public Guid NewSupervisorId { get; set; }
        public MoveUserToAnotherTeamMode Mode { get; set; }
    }

    public class DeleteSupervisorCommandRequest
    {
        public Guid SupervisorId { get; set; }
    }
}
